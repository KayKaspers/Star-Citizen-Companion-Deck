namespace Xeneon.Runtime.Core;

public enum HealthState { UNKNOWN, STARTING, READY, DEGRADED, OFFLINE, ERROR }
public enum WindowMode { Windowed, Fullscreen }
public enum ExternalPlacementMode { ResizeToBay, CenterInBay }

// All desktop geometry is physical pixels, including negative desktop origins.
public sealed record PixelRect(int X, int Y, int Width, int Height);
public sealed record DisplayInfo(string Key, string Name, PixelRect Bounds, PixelRect WorkArea,
    uint Dpi, bool Primary, bool Cloned = false);
public sealed record WindowIdentity(long Handle, int ProcessId, long ProcessStartTicks,
    string ProcessName, string ClassName, string Title, PixelRect Bounds, bool Minimized);
public sealed record ExternalSelector(string ProcessName, string? ClassName = null, string? Title = null)
{
    public void Validate()
    {
        ValidateProcessName(ProcessName);
        if (string.IsNullOrWhiteSpace(ClassName) && string.IsNullOrWhiteSpace(Title))
            throw new ArgumentException("An exact class or title is required to distinguish the companion from control windows.");
        if ((ClassName is not null && string.IsNullOrWhiteSpace(ClassName)) || (Title is not null && string.IsNullOrWhiteSpace(Title)))
            throw new ArgumentException("Optional selectors must be null or non-empty.");
    }
    public static void ValidateProcessName(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName) || processName.IndexOfAny(['/', '\\', '*', '?']) >= 0)
            throw new ArgumentException("An exact process name without path or wildcard is required.");
    }
    public bool Matches(WindowIdentity window) =>
        string.Equals(ProcessName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? ProcessName[..^4] : ProcessName,
            window.ProcessName, StringComparison.OrdinalIgnoreCase)
        && (ClassName is null || ClassName == window.ClassName)
        && (Title is null || Title == window.Title);
}
public sealed record PlacementResult(bool Confirmed, string Code, PixelRect? Observed = null);
public sealed record Diagnostic(string Component, HealthState State, string Code);
// Public UI projection deliberately omits titles, HWNDs, PIDs, PnP paths and raw exception messages.
public sealed record RuntimeSnapshot(int SchemaVersion, DateTimeOffset ObservedAt, HealthState State,
    WindowMode Mode, DisplayInfo? Display, PixelRect? CompanionBay, Diagnostic[] Diagnostics,
    GameLogSnapshot? StarCitizen = null, GameLogSnapshot? OrionEvents = null,
    GameLogSnapshot? CompanionEvents = null, string CompanionName = "Orion");

public interface IDesktop
{
    IReadOnlyList<DisplayInfo> DiscoverDisplays();
    IReadOnlyList<WindowIdentity> FindWindows(ExternalSelector selector);
    PlacementResult Place(WindowIdentity identity, PixelRect bounds, bool resize = true);
    bool IsCurrent(WindowIdentity identity);
}
