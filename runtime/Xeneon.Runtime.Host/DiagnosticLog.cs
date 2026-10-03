using System.Text.Json;

namespace Xeneon.Runtime.Host;

// Bounded, local-only code log. No window titles, paths, identifiers or exception text.
internal sealed class DiagnosticLog : IDisposable
{
    private readonly string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XeneonEdge", "diagnostics");
    public void Write(string state, string code)
    {
        try
        {
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "runtime.jsonl");
            if (File.Exists(path) && new FileInfo(path).Length > 512 * 1024)
                File.Move(path, Path.Combine(directory, "runtime.previous.jsonl"), true);
            File.AppendAllText(path, JsonSerializer.Serialize(new { time = DateTimeOffset.UtcNow, state, code }) + Environment.NewLine);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { /* Logging must not terminate runtime. */ }
    }
    public void Dispose() { }
}
