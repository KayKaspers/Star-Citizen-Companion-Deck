using System.Text.Json;
using Xeneon.Runtime.Core;

int passed = 0;
void Check(string name, Action test) { test(); passed++; Console.WriteLine("PASS " + name); }
void Equal<T>(T expected, T actual) { if (!Equals(expected, actual)) throw new Exception($"Expected {expected}, got {actual}"); }
void Reject(Action action) { try { action(); } catch (ArgumentException) { return; } throw new Exception("Expected validation rejection"); }
var config = new RuntimeConfig(); config.Validate();
var edge = new DisplayInfo("edge", "XENEON EDGE", new(-2560, -720, 2560, 720), new(-2560, -720, 2560, 680), 144, false);
var desktop = new FakeDesktop { Displays = [edge] };
Check("identity independent of index, origin and scale", () => Equal(edge, DisplaySelection.Select([edge], config).Display));
Check("documented XENON spelling", () => Equal("DISPLAY_SELECTED", DisplaySelection.Select([edge with { Name = "XENON EDGE" }], config).Code));
Check("resolution alone never identifies EDGE", () => Equal("DISPLAY_MISSING", DisplaySelection.Select([edge with { Name = "Unrelated ultrawide" }], config).Code));
Check("ambiguous names fail closed", () => Equal("DISPLAY_AMBIGUOUS", DisplaySelection.Select([edge, edge with { Key = "other" }], config).Code));
Check("cloned target fails closed", () => Equal("DISPLAY_CLONED", DisplaySelection.Select([edge with { Cloned = true }], config).Code));
Check("explicit key never falls back", () => Equal("DISPLAY_MISSING", DisplaySelection.Select([edge], config with { DisplayKey = "missing" }).Code));
Check("explicit key resolves identical displays", () => Equal(edge, DisplaySelection.Select([edge, edge with { Key = "other" }], config with { DisplayKey = "edge" }).Display));
Check("relative bay uses physical pixels with negative origin", () => Equal(new PixelRect(-864, -720, 864, 720), config.CompanionArea.On(edge.Bounds)));
Check("windowed uses work area", () => Equal(new PixelRect(-2432, -686, 2304, 612), config.WindowedArea.On(edge.WorkArea)));
Check("non-finite geometry rejected", () => Reject(() => new RelativeRect(double.NaN, 0, 1, 1).Validate()));
Check("out of bounds geometry rejected", () => Reject(() => new RelativeRect(.8, 0, .3, 1).Validate()));
Check("zero geometry rejected", () => Reject(() => new RelativeRect(0, 0, 0, 1).Validate()));
Check("unknown config schema rejected", () => Reject(() => (config with { SchemaVersion = 2 }).Validate()));
Check("too fast refresh rejected", () => Reject(() => (config with { RefreshMilliseconds = 10 }).Validate()));
Check("unknown mode rejected", () => Reject(() => (config with { Mode = (WindowMode)99 }).Validate()));
Check("external process-only selector rejected", () => Reject(() => new ExternalSelector("Orion").Validate()));
Check("external wildcard rejected", () => Reject(() => new ExternalSelector("*", Title: "Companion").Validate()));
Check("null layout rejected", () => Reject(() => (config with { WindowedArea = null! }).Validate()));
Check("config roundtrip", () => Equal(config.CompanionArea, JsonSerializer.Deserialize<RuntimeConfig>(JsonSerializer.Serialize(config, RuntimeConfig.Json), RuntimeConfig.Json)!.CompanionArea));
Check("unknown JSON property rejected", () => { try { JsonSerializer.Deserialize<RuntimeConfig>("{\"unrecognised\":true}", RuntimeConfig.Json); } catch (JsonException) { return; } throw new Exception("Expected rejection"); });
var selector = new ExternalSelector("Fixture", Title: "Companion");
var companion = new WindowIdentity(42, 7, 123, "Fixture", "FixtureClass", "Companion", new(10, 20, 200, 200), false);
Check("exact title excludes control UI", () => Equal(false, selector.Matches(companion with { Title = "Controls" })));
Check("exact process excludes impostor", () => Equal(false, selector.Matches(companion with { ProcessName = "Other" })));
Check("process names containing dots preserved", () => Equal(true, new ExternalSelector("Fixture.App.exe", Title: "Companion").Matches(companion with { ProcessName = "Fixture.App" })));
Check("startup health", () => { using var s = new RuntimeSession(desktop, config); Equal(HealthState.STARTING, s.Snapshot.State); });
Check("switch restores old companion before targeting new variant", () => {
    var d = new FakeDesktop { Displays = [edge], Windows = [companion] };
    using var s = new RuntimeSession(d, config with { ExternalWindow = selector });
    s.Refresh(placeExternal: true);
    Equal(true, s.SelectExternal(new ExternalSelector("Aurora", Title: "Aurora Companion")));
    Equal(companion.Bounds, d.Placements.Last()); Equal(false, s.HasPendingRestoration);
    Equal("EXTERNAL_MISSING", s.Refresh().Diagnostics.Last().Code);
});
Check("optional integration disabled ready", () => { using var s = new RuntimeSession(desktop, config); Equal(HealthState.READY, s.Refresh().State); Equal(0, desktop.Placements.Count); });
Check("missing integration degrades without abort", () => { using var s = new RuntimeSession(desktop, config with { ExternalWindow = selector }); Equal(HealthState.DEGRADED, s.Refresh().State); });
Check("missing display offline", () => { using var s = new RuntimeSession(new FakeDesktop(), config); Equal(HealthState.OFFLINE, s.Refresh().State); });
Check("desktop failure explicit error", () => { using var s = new RuntimeSession(new FakeDesktop { FailDisplay = true }, config); Equal(HealthState.ERROR, s.Refresh().State); });
Check("external failure contained", () => { using var s = new RuntimeSession(new FakeDesktop { Displays = [edge], FailExternal = true }, config with { ExternalWindow = selector }); Equal(HealthState.DEGRADED, s.Refresh().State); });
Check("host sent is not confirmed", () => { using var s = new RuntimeSession(desktop, config); Equal(HealthState.DEGRADED, s.Refresh((_, _) => new(false, "UNCONFIRMED")).State); });
Check("unconfirmed host blocks external movement", () => { var d = new FakeDesktop { Displays = [edge], Windows = [companion] }; using var s = new RuntimeSession(d, config with { ExternalWindow = selector }); s.Refresh((_, _) => new(false, "UNCONFIRMED"), true); Equal(0, d.Placements.Count); });
Check("display query failure releases managed external", () => { var d = new FakeDesktop { Displays = [edge], Windows = [companion] }; using var s = new RuntimeSession(d, config with { ExternalWindow = selector }); s.Refresh(placeExternal: true); d.FailDisplay = true; Equal(HealthState.ERROR, s.Refresh(placeExternal: true).State); Equal(companion.Bounds, d.Placements.Last()); });
Check("host placement failure releases previously managed external", () => { var d = new FakeDesktop { Displays = [edge], Windows = [companion] }; using var s = new RuntimeSession(d, config with { ExternalWindow = selector }); s.Refresh(placeExternal: true); Equal(true, s.HasPendingRestoration); s.Refresh((_, _) => new(false, "HOST_FAILED"), true); Equal(companion.Bounds, d.Placements.Last()); Equal(false, s.HasPendingRestoration); });
Check("unconfirmed release is tracked until observation confirms", () => { var d = new FakeDesktop { Displays = [edge], Windows = [companion] }; using var s = new RuntimeSession(d, config with { ExternalWindow = selector }); s.Refresh(placeExternal: true); d.Confirm = false; s.ReleaseExternal(); Equal(true, s.HasPendingRestoration); d.Confirm = true; s.ReleaseExternal(); Equal(false, s.HasPendingRestoration); });
Check("center placement keeps current native size", () => { var d = new FakeDesktop { Displays = [edge], Windows = [companion] }; using var s = new RuntimeSession(d, config with { Mode = WindowMode.Fullscreen, ExternalWindow = selector, ExternalPlacement = ExternalPlacementMode.CenterInBay }); Equal(HealthState.READY, s.Refresh(placeExternal: true).State); Equal(new PixelRect(-532, -460, 200, 200), d.Placements.First()); });
Check("oversized companion cannot be centered inside bay", () => { var d = new FakeDesktop { Displays = [edge], Windows = [companion with { Bounds = new(0, 0, 900, 200) }] }; using var s = new RuntimeSession(d, config with { Mode = WindowMode.Fullscreen, ExternalWindow = selector, ExternalPlacement = ExternalPlacementMode.CenterInBay }); Equal(HealthState.DEGRADED, s.Refresh(placeExternal: true).State); Equal(0, d.Placements.Count); });
Check("ambiguous external not moved", () => { var d = new FakeDesktop { Displays = [edge], Windows = [companion, companion with { Handle = 43 }] }; using var s = new RuntimeSession(d, config with { ExternalWindow = selector }); Equal(HealthState.DEGRADED, s.Refresh(placeExternal: true).State); Equal(0, d.Placements.Count); });
Check("minimized external not moved", () => { var d = new FakeDesktop { Displays = [edge], Windows = [companion with { Minimized = true }] }; using var s = new RuntimeSession(d, config with { ExternalWindow = selector }); s.Refresh(placeExternal: true); Equal(0, d.Placements.Count); });
Check("observe-only does not move", () => { var d = new FakeDesktop { Displays = [edge], Windows = [companion] }; using var s = new RuntimeSession(d, config with { ExternalWindow = selector }); s.Refresh(); Equal(0, d.Placements.Count); });
Check("restore original after repeated refresh and mode change", () => { var d = new FakeDesktop { Displays = [edge], Windows = [companion] }; using var s = new RuntimeSession(d, config with { ExternalWindow = selector }); s.Refresh(placeExternal: true); s.Mode = WindowMode.Fullscreen; s.Refresh(placeExternal: true); s.ReleaseExternal(); Equal(companion.Bounds, d.Placements.Last()); });
Check("display loss releases companion", () => { var d = new FakeDesktop { Displays = [edge], Windows = [companion] }; using var s = new RuntimeSession(d, config with { ExternalWindow = selector }); s.Refresh(placeExternal: true); d.Displays = []; Equal(HealthState.OFFLINE, s.Refresh(placeExternal: true).State); Equal(companion.Bounds, d.Placements.Last()); });
Check("recycled handle never restored", () => { var d = new FakeDesktop { Displays = [edge], Windows = [companion] }; using var s = new RuntimeSession(d, config with { ExternalWindow = selector }); s.Refresh(placeExternal: true); d.Windows = [companion with { ProcessStartTicks = 456 }]; s.ReleaseExternal(); Equal(1, d.Placements.Count); });
Check("failed restore remains retryable", () => { var d = new FakeDesktop { Displays = [edge], Windows = [companion] }; using var s = new RuntimeSession(d, config with { ExternalWindow = selector }); s.Refresh(placeExternal: true); d.Confirm = false; Equal("EXTERNAL_RESTORE_FAILED", s.ReleaseExternal().Single().Code); d.Confirm = true; Equal("EXTERNAL_RESTORED", s.ReleaseExternal().Single().Code); });
Check("dispose idempotent and offline", () => { var s = new RuntimeSession(desktop, config); s.Dispose(); s.Dispose(); Equal(HealthState.OFFLINE, s.Snapshot.State); });
Console.WriteLine($"{passed} core checks passed");

sealed class FakeDesktop : IDesktop
{
    public IReadOnlyList<DisplayInfo> Displays = [];
    public IReadOnlyList<WindowIdentity> Windows = [];
    public readonly List<PixelRect> Placements = [];
    public bool FailDisplay, FailExternal;
    public bool Confirm = true;
    public IReadOnlyList<DisplayInfo> DiscoverDisplays() => FailDisplay ? throw new InvalidOperationException("private details") : Displays;
    public IReadOnlyList<WindowIdentity> FindWindows(ExternalSelector selector) => FailExternal ? throw new InvalidOperationException("private details") : Windows.Where(selector.Matches).ToArray();
    public bool IsCurrent(WindowIdentity identity) => Windows.Any(w => w.Handle == identity.Handle && w.ProcessId == identity.ProcessId && w.ProcessStartTicks == identity.ProcessStartTicks && w.ClassName == identity.ClassName);
    public PlacementResult Place(WindowIdentity identity, PixelRect bounds, bool resize = true)
    {
        Placements.Add(bounds);
        if (Confirm) Windows = Windows.Select(w => w.Handle == identity.Handle ? w with { Bounds = bounds } : w).ToArray();
        return new(Confirm, Confirm ? "PLACEMENT_CONFIRMED" : "PLACEMENT_UNCONFIRMED", bounds);
    }
}
