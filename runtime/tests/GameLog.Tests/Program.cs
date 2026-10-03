using System.Text;
using Xeneon.Runtime.Core;

int passed = 0;
void Check(string name, Action action) { action(); passed++; Console.WriteLine("PASS " + name); }
void Equal<T>(T expected, T actual) { if (!Equals(expected, actual)) throw new Exception($"Expected {expected}, got {actual}"); }
const string prefix = "<2026-10-03T09:00:00.000Z> [Notice] <Fixture> ";
string directory = Path.Combine(Path.GetTempPath(), "xee-wp026-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(directory);
string file = Path.Combine(directory, "Game.log");
try
{
    Check("missing log is offline and reconnects", () => { using var t = new GameLogTailer(file); Equal(HealthState.OFFLINE, t.Poll().State); File.WriteAllText(file, prefix + "startup\n"); Equal(HealthState.READY, t.Poll().State); Equal(1, t.Snapshot.Events.Length); });
    Check("initial replay distinguished from live append", () => { File.WriteAllText(file, prefix + "history\n"); using var t = new GameLogTailer(file); Equal(true, t.Poll().Events.Single().Historical); File.AppendAllText(file, prefix + "live\n"); Equal(false, t.Poll().Events.Last().Historical); Equal(2L, t.Snapshot.LinesRead); t.Poll(); Equal(2L, t.Snapshot.LinesRead); });
    Check("split UTF8 and partial lines wait for newline", () => { File.WriteAllText(file, ""); using var t = new GameLogTailer(file); t.Poll(); var bytes = Encoding.UTF8.GetBytes(prefix + "Grüße\n"); int split = Array.IndexOf(bytes, (byte)0xc3) + 1; using (var f = new FileStream(file, FileMode.Append)) f.Write(bytes, 0, split); Equal(0, t.Poll().Events.Length); using (var f = new FileStream(file, FileMode.Append)) f.Write(bytes, split, bytes.Length - split); Equal("Grüße", t.Poll().Events.Single().Message); });
    Check("truncation drops previous generation and partial fragment", () => { File.WriteAllText(file, prefix + "old\npartial"); using var t = new GameLogTailer(file); t.Poll(); File.WriteAllText(file, "new\n"); var s = t.Poll(); Equal(2, s.Generation); Equal("new", s.Events.Single().Message); Equal("LOG_TRUNCATED_OR_REWRITTEN", s.Code); });
    Check("same size overwrite detected by checkpoint", () => { File.WriteAllText(file, prefix + "aaa\n"); using var t = new GameLogTailer(file); t.Poll(); File.WriteAllText(file, prefix + "bbb\n"); Equal("bbb", t.Poll().Events.Single().Message); Equal(2, t.Snapshot.Generation); });
    Check("replaced file identity restarts even with same prefix", () => { string id = "one"; File.WriteAllText(file, prefix + "same\n"); using var t = new GameLogTailer(file, _ => id); t.Poll(); id = "two"; File.WriteAllText(file, prefix + "same\n" + prefix + "next\n"); Equal(2, t.Poll().Generation); Equal("same", t.Snapshot.Events.First().Message); });
    Check("sharing denial remains retryable", () => { File.WriteAllText(file, prefix + "line\n"); using var t = new GameLogTailer(file); t.Poll(); using (var locked = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) Equal(HealthState.OFFLINE, t.Poll().State); Equal(HealthState.READY, t.Poll().State); Equal(2, t.Snapshot.Generation); });
    Check("oversized lines discarded without unbounded buffer", () => { File.WriteAllText(file, ""); using var t = new GameLogTailer(file); t.Poll(); File.AppendAllText(file, new string('x', 70000) + "\n" + prefix + "valid\n"); var s = t.Poll(); Equal(1L, s.DroppedLines); Equal("valid", s.Events.Single().Message); });
    Check("retention capped at 500 rows", () => { File.WriteAllText(file, ""); using var t = new GameLogTailer(file); t.Poll(); File.AppendAllText(file, string.Concat(Enumerable.Range(0, 600).Select(i => prefix + i + "\n"))); Equal(500, t.Poll().Events.Length); Equal(600L, t.Snapshot.LinesRead); });
    Check("unknown lines remain uninterpreted", () => { var e = GameLogParser.Parse("entity Foo_Nyx or Drake_Corsair", 1, 1, false); Equal("SYSTEM", e.Category); Equal(false, e.Recognized); Equal(null, e.Time); });
    Check("structured tags classify observations without player claims", () => { var e = GameLogParser.Parse(prefix + "NOT AUTH unknown entity [QuantumTravel]", 1, 1, false); Equal("SHIP", e.Category); Equal("Fixture", e.Source); Equal(true, e.Recognized); });
    Check("nested lambda source brackets preserved", () => { var e = GameLogParser.Parse("<2026-10-03T09:00:00Z> [Notice] <Handler::<lambda_1>::operator ()> Observation [ATC]", 1, 1, false); Equal("Handler::<lambda_1>::operator ()", e.Source); Equal("Observation [ATC]", e.Message); });
    Check("severity error takes precedence", () => { var e = GameLogParser.Parse("<2026-10-03T09:00:00Z> [Error] <QT> failure [QuantumTravel]", 1, 1, false); Equal("ERROR", e.Category); Equal("Error", e.Level); });
    Check("sensitive metadata redacted in UI projection", () => { var e = GameLogParser.Parse(prefix + "User: Person, token=secret 192.0.2.1 test@example.invalid", 1, 1, false); Equal(false, e.Message.Contains("secret")); Equal(false, e.Message.Contains("Person")); Equal(false, e.Message.Contains("192.0.2.1")); });
    Check("initial replay skips partial leading line", () => { File.WriteAllText(file, new string('x', 80000) + "\n" + prefix + "last\n"); using var t = new GameLogTailer(file); Equal("last", t.Poll().Events.Single().Message); });
    Check("relative path rejected", () => { try { (new RuntimeConfig { GameLogPath = "Game.log" }).Validate(); } catch (ArgumentException) { return; } throw new Exception("Expected rejection"); });
    Check("Orion emitted event uses timestamp and named observation", () => {
        var e = OrionEventParser.Parse("{\"schemaVersion\":1,\"type\":\"ship_identified\",\"timestampUtc\":\"2026-10-03T09:00:00Z\",\"name\":\"Fixture ship\"}", 1, 2, true)!;
        Equal("SHIP", e.Category); Equal("Schiff erkannt: Fixture ship", e.Message); Equal(true, e.Historical); Equal(2, e.Generation);
    });
    Check("invalid Orion rows never become reactions", () => {
        foreach (var line in new[] {"bad", "[]", "{\"schemaVersion\":\"1\"}", "{\"schemaVersion\":2}",
            "{\"schemaVersion\":1,\"type\":\"unknown\",\"timestampUtc\":\"2026-10-03T09:00:00Z\"}"})
            Equal<GameLogEvent?>(null, OrionEventParser.Parse(line, 1, 1, false));
    });
    Check("Orion tailer skips malformed row and follows append", () => {
        File.WriteAllText(file, "invalid\n"); using var t = new GameLogTailer(file, parser: OrionEventParser.Parse);
        Equal(0, t.Poll().Events.Length); Equal(1L, t.Snapshot.DroppedLines);
        File.AppendAllText(file, "{\"schemaVersion\":1,\"type\":\"safety_zone_entered\",\"timestampUtc\":\"2026-10-03T09:00:00Z\"}\n");
        Equal("LOCATION", t.Poll().Events.Single().Category); Equal(false, t.Snapshot.Events.Single().Historical);
    });
    Check("Aurora and Orion project identical events with distinct attribution", () => {
        const string line = "{\"schemaVersion\":1,\"type\":\"ship_identified\",\"timestampUtc\":\"2026-10-03T09:00:00Z\",\"name\":\"Fixture ship\"}";
        var a = CompanionEventParser.Parse(line, 1, 1, false, "Aurora")!;
        var o = CompanionEventParser.Parse(line, 1, 1, false, "Orion")!;
        Equal(o.Message, a.Message); Equal(o.Category, a.Category); Equal("AURORA · ship_identified", a.Source);
    });
    Check("companion config rejects mixed variant paths", () => {
        foreach (var c in new[] {new RuntimeConfig { CompanionVariant = "Other" },
            new RuntimeConfig { CompanionVariant = "Aurora", OrionEventsPath = Path.Combine(directory, "Orion-Companion-Events.jsonl") },
            new RuntimeConfig { CompanionVariant = "Aurora", CompanionEventsPath = Path.Combine(directory, "Orion-Companion-Events.jsonl") }}) {
            bool rejected = false; try { c.Validate(); } catch (ArgumentException) { rejected = true; }
            Equal(true, rejected);
        }
        new RuntimeConfig { CompanionVariant = "Aurora", CompanionEventsPath = Path.Combine(directory, "Aurora-Companion-Events.jsonl") }.Validate();
    });
    Console.WriteLine($"{passed} Game.log checks passed");
}
finally
{
    if (!Path.GetFullPath(directory).StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase)
        || !Path.GetFileName(directory).StartsWith("xee-wp026-", StringComparison.Ordinal)) throw new Exception("Unsafe fixture cleanup path");
    Directory.Delete(directory, true);
}
