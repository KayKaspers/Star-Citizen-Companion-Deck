using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Xeneon.Runtime.Core;

namespace Xeneon.Runtime.Windows;

// Scopes must stay on a single thread. Do not await while the native context is overridden.
internal sealed class DpiScope : IDisposable
{
    private readonly nint previous = Native.SetThreadDpiAwarenessContext(new nint(-4)); // PER_MONITOR_AWARE_V2
    public DpiScope() { if (previous == 0) throw new Win32Exception(Marshal.GetLastWin32Error()); }
    public void Dispose() => Native.SetThreadDpiAwarenessContext(previous);
}

public sealed class WindowsDesktop : IDesktop
{
    private static PixelRect Rect(Native.Rect r) => new(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
    public IReadOnlyList<DisplayInfo> DiscoverDisplays()
    {
        using var dpi = new DpiScope();
        var names = ReadDisplayNames();
        var displays = new List<DisplayInfo>(); Exception? failure = null;
        bool success = Native.EnumDisplayMonitors(0, 0, (monitor, _, _, _) =>
        {
            try
            {
                var info = new Native.MonitorInfo { Size = Marshal.SizeOf<Native.MonitorInfo>() };
                if (!Native.GetMonitorInfoW(monitor, ref info)) throw new Win32Exception(Marshal.GetLastWin32Error());
                if (Native.GetDpiForMonitor(monitor, 0, out uint x, out _) != 0) x = 0; // Unknown, never pretend 96.
                var targets = names.GetValueOrDefault(info.Device) ?? [];
                displays.Add(new(info.Device, targets.Count == 1 ? targets[0] : "UNKNOWN", Rect(info.Monitor),
                    Rect(info.Work), x, (info.Flags & 1) != 0, targets.Count > 1));
                return true;
            }
            catch (Exception e) { failure = e; return false; }
        }, 0);
        if (failure is not null) throw failure;
        if (!success) throw new Win32Exception(Marshal.GetLastWin32Error());
        return displays;
    }

    private static Dictionary<string, List<string>> ReadDisplayNames()
    {
        const uint active = 2;
        // Topology may change between sizing and querying. Retry ERROR_INSUFFICIENT_BUFFER.
        for (int attempt = 0; attempt < 4; attempt++)
        {
            int error = Native.GetDisplayConfigBufferSizes(active, out uint paths, out uint modes);
            if (error != 0) throw new Win32Exception(error);
            var pathInfo = new Native.DisplayPath[paths]; var modeInfo = new Native.ModeInfo[modes];
            error = Native.QueryDisplayConfig(active, ref paths, pathInfo, ref modes, modeInfo, 0);
            if (error == 122) continue;
            if (error != 0) throw new Win32Exception(error);
            var names = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in pathInfo.Take((int)paths))
            {
                var source = new Native.SourceName { Header = new() { Type = 1, Size = (uint)Marshal.SizeOf<Native.SourceName>(), Adapter = path.Source.Adapter, Id = path.Source.Id } };
                var target = new Native.TargetName { Header = new() { Type = 2, Size = (uint)Marshal.SizeOf<Native.TargetName>(), Adapter = path.Target.Adapter, Id = path.Target.Id } };
                error = Native.SourceDeviceInfo(ref source);
                if (error != 0) throw new Win32Exception(error);
                error = Native.TargetDeviceInfo(ref target);
                if (error != 0) throw new Win32Exception(error);
                if (!names.TryGetValue(source.Name, out var values)) names[source.Name] = values = [];
                values.Add(string.IsNullOrWhiteSpace(target.Name) ? "UNKNOWN" : target.Name);
                // Target device instance paths and EDID IDs never leave this adapter.
            }
            return names;
        }
        throw new Win32Exception(122);
    }

    public IReadOnlyList<WindowIdentity> FindWindows(ExternalSelector selector)
    {
        selector.Validate();
        return EnumerateWindows().Where(selector.Matches).ToArray();
    }

    // Read-only commissioning output. Process-only matching is never accepted by placement policy.
    public IReadOnlyList<WindowIdentity> InspectWindows(string processName)
    {
        ExternalSelector.ValidateProcessName(processName);
        string name = processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? processName[..^4] : processName;
        return EnumerateWindows().Where(w => string.Equals(name, w.ProcessName, StringComparison.OrdinalIgnoreCase)).ToArray();
    }

    private IReadOnlyList<WindowIdentity> EnumerateWindows()
    {
        using var dpi = new DpiScope(); var result = new List<WindowIdentity>(); Exception? failure = null;
        bool success = Native.EnumWindows((window, _) =>
        {
            try
            {
            var identity = ReadWindow(window);
            if (identity is not null && Native.IsWindowVisible(window)) result.Add(identity);
            return true;
            }
            catch (Exception e) { failure = e; return false; }
        }, 0);
        if (failure is not null) throw failure;
        if (!success) throw new Win32Exception(Marshal.GetLastWin32Error());
        return result;
    }

    public WindowIdentity? ReadWindow(nint window)
    {
        using var dpi = new DpiScope();
        if (!Native.IsWindow(window)) return null;
        Native.GetWindowThreadProcessId(window, out uint pid);
        try
        {
            using var process = Process.GetProcessById((int)pid);
            var title = new StringBuilder(1024); var className = new StringBuilder(256);
            Native.GetWindowTextW(window, title, title.Capacity); Native.GetClassNameW(window, className, className.Capacity);
            if (!Native.GetWindowRect(window, out var bounds)) return null;
            return new(window.ToInt64(), (int)pid, process.StartTime.ToUniversalTime().Ticks,
                process.ProcessName, className.ToString(), title.ToString(), Rect(bounds), Native.IsIconic(window));
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException or Win32Exception) { return null; }
    }

    public bool IsCurrent(WindowIdentity identity)
    {
        var current = ReadWindow(new nint(identity.Handle));
        return current is not null && current.ProcessId == identity.ProcessId
            && current.ProcessStartTicks == identity.ProcessStartTicks && current.ClassName == identity.ClassName
            && current.ProcessName == identity.ProcessName && current.Title == identity.Title;
    }

    public PlacementResult Place(WindowIdentity identity, PixelRect bounds, bool resize = true)
    {
        using var dpi = new DpiScope();
        if (bounds.Width <= 0 || bounds.Height <= 0) return new(false, "PLACEMENT_INVALID");
        if (!IsCurrent(identity)) return new(false, "WINDOW_STALE");
        if (Native.IsIconic(new nint(identity.Handle))) return new(false, "WINDOW_MINIMIZED");
        var before = ReadWindow(new nint(identity.Handle));
        if (before?.Bounds == bounds) return new(true, "PLACEMENT_CONFIRMED", bounds);
        // External apps retain parent, styles, owner, visibility, z-order and focus.
        uint flags = 0x0004 | 0x0010 | 0x0200 | 0x4000; // NOZORDER | NOACTIVATE | NOOWNERZORDER | ASYNC
        if (!resize) flags |= 0x0001; // NOSIZE: the external application retains its sizing policy.
        if (!Native.SetWindowPos(new nint(identity.Handle), 0, bounds.X, bounds.Y, bounds.Width, bounds.Height, flags))
            return new(false, "PLACEMENT_REJECTED");
        var observed = ReadWindow(new nint(identity.Handle));
        // Async cross-process requests can remain pending until the next refresh. Never claim confirmed on send alone.
        return observed is not null && IsCurrent(identity) && observed.Bounds == bounds
            ? new(true, "PLACEMENT_CONFIRMED", observed.Bounds)
            : new(false, "PLACEMENT_UNCONFIRMED", observed?.Bounds);
    }
}
