using System.Text;
using System.Text.RegularExpressions;

namespace Xeneon.Runtime.Core;

public sealed record GameLogEvent(long Sequence, int Generation, DateTimeOffset? Time, string Level,
    string Category, string Source, string Message, bool Historical, bool Recognized);
public sealed record GameLogSnapshot(int SchemaVersion, HealthState State, string Code, int Generation,
    long LinesRead, long DroppedLines, DateTimeOffset? LastReadAt, GameLogEvent[] Events);

public static class GameLogParser
{
    private static readonly Regex Header = new(@"^<(?<time>[^>]{1,64})>\s*(?:\[(?<level>Notice|Warning|Error|Fatal|Info|Debug)\]\s*)?(?<message>.*)$", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
    private static readonly Regex Sensitive = new(@"(?i)\b(?:User|Client|account(?:id)?|token|session(?:id)?|authorization|password)\s*[:=]\s*[^,\s]+|\b(?:\d{1,3}\.){3}\d{1,3}\b|[\w.+-]+@[\w.-]+\.[A-Za-z]{2,}|\b[0-9a-f]{32,}\b", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
    public static GameLogEvent Parse(string line, long sequence, int generation, bool historical)
    {
        var match = Header.Match(line);
        DateTimeOffset? time = null;
        if (match.Success && DateTimeOffset.TryParse(match.Groups["time"].Value, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal, out var parsed)) time = parsed;
        string level = match.Groups["level"].Value;
        if (level.Length == 0) level = "UNKNOWN";
        string source = "";
        string message = match.Success ? match.Groups["message"].Value : line;
        if (match.Success && message.StartsWith('<'))
        {
            int depth = 0;
            for (int i = 0; i < message.Length; i++)
            {
                if (message[i] == '<') depth++;
                else if (message[i] == '>' && --depth == 0)
                {
                    source = message[1..i]; message = message[(i + 1)..].TrimStart(); break;
                }
            }
        }
        // Categories classify an observed log row. They never establish the local player's current state.
        string category = level is "Error" or "Fatal" ? "ERROR"
            : line.Contains("[Combat]", StringComparison.Ordinal) || line.Contains("[Damage]", StringComparison.Ordinal) ? "COMBAT"
            : line.Contains("[Location]", StringComparison.Ordinal) ? "LOCATION"
            : line.Contains("[Player]", StringComparison.Ordinal) ? "PLAYER"
            : line.Contains("[ATC]", StringComparison.Ordinal) || line.Contains("[QuantumTravel]", StringComparison.Ordinal) || line.Contains("[Vehicle]", StringComparison.Ordinal) ? "SHIP"
            : "SYSTEM";
        return new(sequence, generation, time, level, category, Redact(source), Redact(message), historical,
            match.Success && (source.Length > 0 || level != "UNKNOWN"));
    }
    private static string Redact(string text)
    {
        string safe = Sensitive.Replace(text, "[REDACTED]");
        return safe.Length > 1200 ? safe[..1200] + "…" : safe;
    }
}

// Polling opens a read-only shared handle each time: the game may append, rename or replace its file freely.
// Work per poll, line bytes and retained rows are bounded. Only newline-complete UTF-8 lines are emitted.
public sealed class GameLogTailer(string path, Func<FileStream, string>? identityProvider = null,
    Func<string, long, int, bool, GameLogEvent?>? parser = null) : IDisposable
{
    private const int MaxRead = 256 * 1024, InitialReplay = 64 * 1024, MaxLine = 64 * 1024;
    private readonly Queue<GameLogEvent> events = [];
    private readonly List<byte> pending = [];
    private string? identity;
    private byte[] checkpoint = [];
    private long offset, initialEnd, sequence, lines, dropped;
    private int generation;
    private bool skipLine, disconnected, disposed;
    public GameLogSnapshot Snapshot { get; private set; } = new(1, HealthState.STARTING, "LOG_STARTING", 0, 0, 0, null, []);

    public GameLogSnapshot Poll()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        try
        {
            using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            string current = identityProvider?.Invoke(file) ?? File.GetCreationTimeUtc(path).Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string code = "LOG_ATTACHED";
            bool first = identity is null;
            bool replaced = !first && (current != identity || disconnected);
            bool truncated = !first && !replaced && (file.Length < offset || !CheckpointMatches(file));
            if (first || replaced || truncated)
            {
                generation++; events.Clear(); pending.Clear(); checkpoint = [];
                offset = first ? Math.Max(0, file.Length - InitialReplay) : 0;
                skipLine = offset > 0; initialEnd = file.Length;
                identity = current; disconnected = false;
                code = first ? "LOG_ATTACHED" : replaced ? "LOG_REATTACHED_OR_REPLACED" : "LOG_TRUNCATED_OR_REWRITTEN";
            }
            file.Position = offset;
            var buffer = new byte[MaxRead]; int count = file.Read(buffer, 0, buffer.Length);
            for (int i = 0; i < count; i++)
            {
                byte b = buffer[i]; long endOffset = offset + i + 1;
                if (b == 10)
                {
                    if (!skipLine)
                    {
                        string line = Encoding.UTF8.GetString(pending.ToArray()).TrimEnd('\r').TrimStart('\uFEFF');
                        if (line.Length > 0)
                        {
                            lines++;
                            var row = parser is null ? GameLogParser.Parse(line, ++sequence, generation, endOffset <= initialEnd)
                                : parser(line, ++sequence, generation, endOffset <= initialEnd);
                            if (row is not null) events.Enqueue(row); else dropped++;
                        }
                        while (events.Count > 500) events.Dequeue();
                    }
                    pending.Clear(); skipLine = false;
                }
                else if (!skipLine)
                {
                    if (pending.Count == MaxLine) { pending.Clear(); skipLine = true; dropped++; }
                    else pending.Add(b);
                }
            }
            offset += count;
            int length = (int)Math.Min(64, offset); checkpoint = new byte[length];
            file.Position = offset - length; file.ReadExactly(checkpoint);
            return Snapshot = new(1, HealthState.READY, code, generation, lines, dropped, DateTimeOffset.UtcNow, events.ToArray());
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            disconnected = true;
            string code = e is FileNotFoundException or DirectoryNotFoundException ? "LOG_MISSING" : "LOG_READ_UNAVAILABLE";
            return Snapshot = Snapshot with { State = HealthState.OFFLINE, Code = code };
        }
    }

    private bool CheckpointMatches(FileStream file)
    {
        if (checkpoint.Length == 0) return true;
        file.Position = offset - checkpoint.Length;
        var observed = new byte[checkpoint.Length];
        return file.Read(observed, 0, observed.Length) == observed.Length && observed.SequenceEqual(checkpoint);
    }
    public void Dispose() { disposed = true; pending.Clear(); events.Clear(); }
}
