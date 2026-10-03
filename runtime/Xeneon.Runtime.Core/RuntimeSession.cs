namespace Xeneon.Runtime.Core;

// The Windows host owns lifecycle/UI; this coordinator owns policy and observed health.
public sealed class RuntimeSession : IDisposable
{
    private readonly IDesktop desktop;
    private readonly RuntimeConfig config;
    private ExternalSelector? externalWindow;
    private readonly Dictionary<WindowIdentity, PixelRect> originals = [];
    private bool disposed;
    public bool HasPendingRestoration => originals.Count > 0;
    public WindowMode Mode { get; set; }
    public RuntimeSnapshot Snapshot { get; private set; } = new(1, DateTimeOffset.UtcNow,
        HealthState.UNKNOWN, WindowMode.Windowed, null, null, []);
    public RuntimeSession(IDesktop desktop, RuntimeConfig config)
    {
        config.Validate(); this.desktop = desktop; this.config = config; Mode = config.Mode;
        externalWindow = config.ExternalWindow;
        Snapshot = Snapshot with { State = HealthState.STARTING, Mode = config.Mode };
    }

    public RuntimeSnapshot Refresh(Func<DisplayInfo, WindowMode, PlacementResult>? placeHost = null,
        bool placeExternal = false)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        var diagnostics = new List<Diagnostic>();
        DisplayInfo? display = null; PixelRect? bay = null; bool hostConfirmed = true;
        try
        {
            var selection = DisplaySelection.Select(desktop.DiscoverDisplays(), config);
            display = selection.Display;
            diagnostics.Add(new("display", display is null ? HealthState.OFFLINE : HealthState.READY, selection.Code));
            if (display is null || !placeExternal) diagnostics.AddRange(ReleaseExternal());
            if (display is not null)
            {
                var hostBounds = Mode == WindowMode.Fullscreen ? display.Bounds : config.WindowedArea.On(display.WorkArea);
                bay = config.CompanionArea.On(hostBounds);
                if (placeHost is not null)
                {
                    var host = placeHost(display, Mode);
                    hostConfirmed = host.Confirmed;
                    diagnostics.Add(new("host", host.Confirmed ? HealthState.READY : HealthState.DEGRADED, host.Code));
                    if (!hostConfirmed) diagnostics.AddRange(ReleaseExternal());
                }
            }
            if (externalWindow is not null)
            {
                try
                {
                    var matches = desktop.FindWindows(externalWindow);
                    string code; HealthState state;
                    if (matches.Count == 0) { code = "EXTERNAL_MISSING"; state = HealthState.DEGRADED; }
                    else if (matches.Count != 1) { code = "EXTERNAL_AMBIGUOUS"; state = HealthState.DEGRADED; }
                    else if (matches[0].Minimized) { code = "EXTERNAL_MINIMIZED"; state = HealthState.DEGRADED; }
                    else if (placeExternal && bay is not null && hostConfirmed)
                    {
                        var window = matches[0];
                        bool resize = config.ExternalPlacement == ExternalPlacementMode.ResizeToBay;
                        if (!resize && (window.Bounds.Width > bay.Width || window.Bounds.Height > bay.Height))
                        {
                            code = "EXTERNAL_TOO_LARGE_FOR_BAY"; state = HealthState.DEGRADED;
                        }
                        else
                        {
                        var target = resize ? bay : new PixelRect(bay.X + (bay.Width - window.Bounds.Width) / 2,
                            bay.Y + (bay.Height - window.Bounds.Height) / 2, window.Bounds.Width, window.Bounds.Height);
                        // Keep only live identities; a recycled handle must never restore an unrelated window.
                        foreach (var old in originals.Keys.Where(k => !desktop.IsCurrent(k)).ToArray()) originals.Remove(old);
                        var original = originals.Keys.FirstOrDefault(k => SameIdentity(k, window));
                        if (original is null) originals.Add(window, window.Bounds);
                        var result = desktop.Place(window, target, resize);
                        code = result.Code; state = result.Confirmed ? HealthState.READY : HealthState.DEGRADED;
                        }
                    }
                    else { code = "EXTERNAL_DETECTED_NOT_POSITIONED"; state = HealthState.UNKNOWN; }
                    diagnostics.Add(new("externalWindow", state, code));
                }
                catch (Exception e) when (e is not OutOfMemoryException)
                { diagnostics.Add(new("externalWindow", HealthState.DEGRADED, "EXTERNAL_OPERATION_FAILED")); }
            }
            else diagnostics.Add(new("externalWindow", HealthState.UNKNOWN, "EXTERNAL_DISABLED"));
        }
        catch (Exception e) when (e is not OutOfMemoryException)
        {
            display = null; bay = null;
            diagnostics.AddRange(ReleaseExternal());
            diagnostics.Add(new("runtime", HealthState.ERROR, "DESKTOP_OPERATION_FAILED"));
        }
        var health = diagnostics.Any(d => d.State == HealthState.ERROR) ? HealthState.ERROR
            : display is null ? HealthState.OFFLINE
            : diagnostics.Any(d => d.State == HealthState.DEGRADED) ? HealthState.DEGRADED : HealthState.READY;
        return Snapshot = new(1, DateTimeOffset.UtcNow, health, Mode, display, bay, diagnostics.ToArray());
    }

    private static bool SameIdentity(WindowIdentity a, WindowIdentity b) => a.Handle == b.Handle
        && a.ProcessId == b.ProcessId && a.ProcessStartTicks == b.ProcessStartTicks && a.ClassName == b.ClassName;

    public Diagnostic[] ReleaseExternal()
    {
        var results = new List<Diagnostic>();
        foreach (var pair in originals.ToArray())
        {
            try
            {
                if (!desktop.IsCurrent(pair.Key)) { originals.Remove(pair.Key); continue; }
                var result = desktop.Place(pair.Key, pair.Value);
                if (result.Confirmed) originals.Remove(pair.Key);
                results.Add(new("externalWindow", result.Confirmed ? HealthState.READY : HealthState.DEGRADED,
                    result.Confirmed ? "EXTERNAL_RESTORED" : "EXTERNAL_RESTORE_FAILED"));
            }
            catch (Exception e) when (e is not OutOfMemoryException)
            { results.Add(new("externalWindow", HealthState.DEGRADED, "EXTERNAL_RESTORE_FAILED")); }
        }
        // Keep failed restores available for another release attempt while this session is alive.
        return results.ToArray();
    }
    public bool SelectExternal(ExternalSelector selector)
    {
        selector.Validate();
        ReleaseExternal();
        if (HasPendingRestoration) return false;
        externalWindow = selector;
        return true;
    }
    public void Dispose()
    {
        if (disposed) return;
        ReleaseExternal(); disposed = true;
        Snapshot = Snapshot with { State = HealthState.OFFLINE, ObservedAt = DateTimeOffset.UtcNow };
    }
}
