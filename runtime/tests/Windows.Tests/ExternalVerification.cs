using System.Diagnostics;
using Xeneon.Runtime.Core;
using Xeneon.Runtime.Windows;

// Explicit commissioning command. Never runs as part of the default test suite.
internal static class ExternalVerification
{
    public static int Run(string processName, string title, ExternalPlacementMode mode = ExternalPlacementMode.ResizeToBay)
    {
        var desktop = new WindowsDesktop();
        var selector = new ExternalSelector(processName, Title: title);
        selector.Validate();
        var selected = desktop.FindWindows(selector);
        if (selected.Count != 1) { Console.Error.WriteLine("EXTERNAL_SELECTION_NOT_UNIQUE"); return 1; }
        var original = selected.Single();
        var untouched = desktop.InspectWindows(processName).Where(w => w.Handle != original.Handle).ToArray();
        var config = new RuntimeConfig { Mode = WindowMode.Fullscreen, ExternalWindow = selector, ExternalPlacement = mode };
        if (DisplaySelection.Select(desktop.DiscoverDisplays(), config).Display is null)
        { Console.Error.WriteLine("DISPLAY_SELECTION_NOT_UNIQUE"); return 1; }
        using var session = new RuntimeSession(desktop, config);
        using var driver = new QuietForm { Text = "XEE external commissioning" };
        int exit = 0;
        driver.Shown += async (_, _) =>
        {
            try
            {
                var timer = Stopwatch.StartNew();
                RuntimeSnapshot snapshot;
                do
                {
                    snapshot = session.Refresh(placeExternal: true);
                    if (snapshot.State == HealthState.READY) break;
                    if (timer.ElapsedMilliseconds > 3000)
                    {
                        Console.WriteLine("Requested: " + snapshot.CompanionBay);
                        Console.WriteLine("Observed: " + desktop.ReadWindow(new nint(original.Handle))?.Bounds);
                        Console.WriteLine("Diagnostics: " + string.Join(',', snapshot.Diagnostics.Select(d => d.Code)));
                        throw new Exception("EXTERNAL_PLACEMENT_UNCONFIRMED");
                    }
                    await Task.Delay(50);
                } while (true);
                Console.WriteLine("PASS live companion placement observed: " + desktop.ReadWindow(new nint(original.Handle))?.Bounds);
                foreach (var other in untouched)
                    if (!desktop.IsCurrent(other) || desktop.ReadWindow(new nint(other.Handle))?.Bounds != other.Bounds)
                        throw new Exception("UNSELECTED_WINDOW_CHANGED");
                Console.WriteLine("PASS unselected control windows unchanged");
            }
            catch (Exception e) { Console.Error.WriteLine(e.Message); exit = 1; }
            finally
            {
                var timer = Stopwatch.StartNew();
                do
                {
                    session.ReleaseExternal();
                    if (!session.HasPendingRestoration || timer.ElapsedMilliseconds > 1500) break;
                    await Task.Delay(50);
                } while (true);
                if (desktop.ReadWindow(new nint(original.Handle))?.Bounds != original.Bounds)
                { Console.Error.WriteLine("EXTERNAL_RESTORE_UNCONFIRMED"); exit = 1; }
                else Console.WriteLine("PASS original companion bounds restored");
                if (untouched.All(w => desktop.IsCurrent(w) && desktop.ReadWindow(new nint(w.Handle))?.Bounds == w.Bounds))
                    Console.WriteLine("PASS unselected control windows unchanged after cleanup");
                else { Console.Error.WriteLine("UNSELECTED_WINDOW_CHANGED"); exit = 1; }
                driver.Close();
            }
        };
        Application.Run(driver); return exit;
    }
}
