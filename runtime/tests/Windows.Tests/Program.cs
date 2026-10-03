using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using Xeneon.Runtime.Core;
using Xeneon.Runtime.Host;
using Xeneon.Runtime.Windows;

internal static class Program
{
    private static int passed;
    [STAThread]
    private static int Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        if (args.Length == 2 && args[0] == "--inspect-log")
        {
            using var reader = new GameLogTailer(args[1], LogFileIdentity.Read);
            var observed = reader.Poll();
            Console.WriteLine(JsonSerializer.Serialize(new { observed.State, observed.Code, observed.Generation,
                observed.LinesRead, observed.DroppedLines, Categories = observed.Events.GroupBy(e => e.Category).ToDictionary(g => g.Key, g => g.Count()) }, RuntimeConfig.Json));
            return observed.State == HealthState.READY ? 0 : 1;
        }
        if (args.Length == 3 && args[0] == "--verify-external")
            return ExternalVerification.Run(args[1], args[2]);
        if (args.Length == 3 && args[0] == "--verify-external-centered")
            return ExternalVerification.Run(args[1], args[2], ExternalPlacementMode.CenterInBay);
        if (args.Length == 2 && args[0] == "--fixture")
        {
            Application.Run(new QuietForm { Text = args[1], Width = 300, Height = 240 }); return 0;
        }
        int exit = 0;
        var desktop = new WindowsDesktop();
        var displays = desktop.DiscoverDisplays();
        Check("native active display enumeration", displays.Count > 0 && displays.All(d => d.Bounds.Width > 0 && d.Bounds.Height > 0));
        var selected = DisplaySelection.Select(displays, new RuntimeConfig()).Display ?? displays.First(d => !d.Cloned);
        string feedDirectory = Path.Combine(Path.GetTempPath(), "xee-wp026-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(feedDirectory);
        string feedPath = Path.Combine(feedDirectory, "Game.log");
        File.WriteAllText(feedPath, "<2026-10-03T09:00:00Z> [Notice] <OwnedFixture> Initial synthetic event [Player]\n");
        string orionPath = Path.Combine(feedDirectory, "Orion-Companion-Events.jsonl");
        string auroraPath = Path.Combine(feedDirectory, "Aurora-Companion-Events.jsonl");
        const string companionFixture = "{\"schemaVersion\":1,\"type\":\"safety_zone_entered\",\"timestampUtc\":\"2026-10-03T09:00:00Z\"}\n";
        File.WriteAllText(orionPath, companionFixture); File.WriteAllText(auroraPath, companionFixture);
        var config = new RuntimeConfig { DisplayKey = selected.Key, Mode = WindowMode.Fullscreen, RefreshMilliseconds = 500, GameLogPath = feedPath, CompanionEventsPath = orionPath };
        using var host = new QuietRuntimeForm(desktop, config);
        host.Shown += async (_, _) =>
        {
            Process? fixture = null;
            try
            {
                await Until(() => host.WebReady, "WebView2 initialization", 20000);
                await Until(() => host.CurrentState.Diagnostics.Any(d => d.Component == "host" && d.State == HealthState.READY), "host placement");
                Check("host fullscreen equals selected physical bounds", desktop.ReadWindow(host.Handle)!.Bounds == selected.Bounds);
                var ui = await host.WebView.CoreWebView2.ExecuteScriptAsync("document.getElementById('health').textContent");
                Check("native snapshot reaches web UI", JsonSerializer.Deserialize<string>(ui) == "Bereit");
                Check("native companion snapshot contains owned Orion event", host.CurrentState.CompanionEvents?.Events.Single().Source.StartsWith("ORION") == true);
                var reactionText = await host.WebView.CoreWebView2.ExecuteScriptAsync("document.getElementById('feed').textContent");
                Check("default native terminal shows companion reactions", JsonSerializer.Deserialize<string>(reactionText)!.Contains("Sicherheitszone betreten"));
                Check("real read-only file identity and initial feed", host.CurrentState.StarCitizen?.Events.Single().Message.Contains("Initial synthetic event") == true);
                File.AppendAllText(feedPath, "<2026-10-03T09:00:01Z> [Notice] <OwnedFixture> Appended synthetic event [ATC]\n");
                await Until(() => host.CurrentState.StarCitizen?.Events.Last().Message.Contains("Appended synthetic event") == true, "native host tails synthetic append");
                await host.WebView.CoreWebView2.ExecuteScriptAsync("document.getElementById('extra-logs').click()");
                var feedText = await host.WebView.CoreWebView2.ExecuteScriptAsync("document.getElementById('feed').textContent");
                Check("appended event reaches terminal UI", JsonSerializer.Deserialize<string>(feedText)!.Contains("Appended synthetic event"));
                await host.WebView.CoreWebView2.ExecuteScriptAsync("document.querySelector('[data-command=toggle-mode]').click()");
                await Until(() => host.CurrentState.Mode == WindowMode.Windowed, "web command switches mode");
                Check("windowed placement uses work area", desktop.ReadWindow(host.Handle)!.Bounds == config.WindowedArea.On(selected.WorkArea));
                using (var placementProbe = new QuietForm { Text = "XEE owned DPI probe" })
                {
                    placementProbe.Show();
                    foreach (var display in displays.Where(d => !d.Cloned))
                    {
                        var target = new PixelRect(display.Bounds.X + 40, display.Bounds.Y + 40, 640, 300);
                        // A second request settles a WM_DPICHANGED suggested-rectangle adjustment, if any.
                        desktop.Place(desktop.ReadWindow(placementProbe.Handle)!, target);
                        var moved = desktop.Place(desktop.ReadWindow(placementProbe.Handle)!, target);
                        Check("owned window physical placement at DPI " + display.Dpi, moved.Confirmed);
                        uint actualDpi = GetDpiForWindow(placementProbe.Handle);
                        Check("owned window DPI follows target monitor", display.Dpi == 0 || actualDpi == display.Dpi);
                    }
                    placementProbe.Close();
                }
                string title = "XEE test companion " + Guid.NewGuid().ToString("N");
                var start = new ProcessStartInfo(Environment.ProcessPath!)
                {
                    // The fixture itself is transparent and never activates; SW_HIDE would exclude it from visible-window discovery.
                    UseShellExecute = false, CreateNoWindow = true
                };
                if (string.Equals(Path.GetFileNameWithoutExtension(Environment.ProcessPath), "dotnet", StringComparison.OrdinalIgnoreCase))
                    start.ArgumentList.Add(typeof(Program).Assembly.Location);
                start.ArgumentList.Add("--fixture"); start.ArgumentList.Add(title);
                fixture = Process.Start(start) ?? throw new Exception("Fixture process failed");
                var selector = new ExternalSelector(fixture.ProcessName, Title: title);
                await Until(() => desktop.FindWindows(selector).Count == 1, "independent fixture detection");
                var window = desktop.FindWindows(selector).Single();
                Check("separate process selected by exact title", window.ProcessId == fixture.Id);
                Check("read-only process inspection finds fixture metadata", desktop.InspectWindows(fixture.ProcessName + ".exe").Any(w => w.Title == title && w.ProcessId == fixture.Id));
                Check("wrong title excludes control surface", desktop.FindWindows(selector with { Title = "other" }).Count == 0);
                var foreground = GetForegroundWindow(); var parent = GetParent(new nint(window.Handle));
                var style = GetWindowLongPtrW(new nint(window.Handle), -16); var extendedStyle = GetWindowLongPtrW(new nint(window.Handle), -20);
                using var session = new RuntimeSession(desktop, config with { ExternalWindow = selector });
                session.Refresh(placeExternal: true);
                Check("position request preserves foreground", GetForegroundWindow() == foreground);
                await Until(() => session.Refresh(placeExternal: true).State == HealthState.READY, "cross-process placement confirmation");
                Check("external physical bounds match companion bay", desktop.ReadWindow(new nint(window.Handle))!.Bounds == config.CompanionArea.On(selected.Bounds));
                Check("external parent and window styles unchanged", GetParent(new nint(window.Handle)) == parent
                    && GetWindowLongPtrW(new nint(window.Handle), -16) == style && GetWindowLongPtrW(new nint(window.Handle), -20) == extendedStyle);
                session.ReleaseExternal();
                await Until(() => desktop.ReadWindow(new nint(window.Handle))!.Bounds == window.Bounds, "external original bounds restoration");
                Check("recovery bounds fit small work areas", RuntimeForm.RecoveryBounds(new(-10, -20, 30, 20)) == new PixelRect(4, -11, 2, 2));
                using (var managedHost = new QuietRuntimeForm(desktop, config with { ExternalWindow = selector }, true))
                {
                    managedHost.Show();
                    await Until(() => managedHost.WebReady && managedHost.CurrentState.Diagnostics.Any(d => d.Component == "externalWindow" && d.State == HealthState.READY), "managed host external placement");
                    managedHost.Close();
                    await Until(() => managedHost.IsDisposed, "managed host completes bounded close", 5000);
                    Check("normal host close confirms external restoration", desktop.ReadWindow(new nint(window.Handle))!.Bounds == window.Bounds);
                }
                fixture.Kill(); await fixture.WaitForExitAsync();
                Check("closed fixture handle is stale", !desktop.IsCurrent(window));
                Check("missing external produces degraded health", session.Refresh().State == HealthState.DEGRADED);
                using (var auroraHost = new QuietRuntimeForm(desktop, config with { CompanionVariant = "Aurora", CompanionEventsPath = auroraPath }))
                {
                    auroraHost.Show();
                    await Until(() => auroraHost.WebReady && auroraHost.CurrentState.CompanionEvents?.Events.Length == 1, "native Aurora fixture feed");
                    Check("native Aurora attribution", auroraHost.CurrentState.CompanionName == "Aurora"
                        && auroraHost.CurrentState.CompanionEvents!.Events.Single().Source.StartsWith("AURORA"));
                    var auroraTitle = await auroraHost.WebView.CoreWebView2.ExecuteScriptAsync("document.getElementById('companion-heading').textContent");
                    Check("native Aurora projection reaches German UI", JsonSerializer.Deserialize<string>(auroraTitle)!.Contains("AURORA"));
                    auroraHost.Close();
                    await Until(() => auroraHost.IsDisposed, "Aurora fixture closes normally");
                }
                var navigation = new TaskCompletionSource<bool>();
                host.WebView.CoreWebView2.NavigationStarting += (_, e) => { if (e.Uri == "https://example.invalid/") navigation.TrySetResult(e.Cancel); };
                host.WebView.CoreWebView2.Navigate("https://example.invalid/");
                Check("off-origin navigation blocked", await navigation.Task.WaitAsync(TimeSpan.FromSeconds(5)));
                Console.WriteLine($"{passed} Windows/host checks passed");
            }
            catch (Exception e) { Console.Error.WriteLine(e); exit = 1; }
            finally
            {
                if (fixture is not null) { if (!fixture.HasExited) fixture.Kill(); fixture.Dispose(); }
                host.Close();
            }
        };
        Application.Run(host);
        if (!Path.GetFullPath(feedDirectory).StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase)
            || !Path.GetFileName(feedDirectory).StartsWith("xee-wp026-", StringComparison.Ordinal)) throw new Exception("Unsafe fixture cleanup path");
        Directory.Delete(feedDirectory, true);
        return exit;
    }
    private static async Task Until(Func<bool> condition, string name, int milliseconds = 5000)
    {
        var timer = Stopwatch.StartNew();
        while (!condition()) { if (timer.ElapsedMilliseconds > milliseconds) throw new Exception("Timeout: " + name); await Task.Delay(50); }
        Check(name, true);
    }
    private static void Check(string name, bool condition)
    {
        if (!condition) throw new Exception("FAIL " + name);
        passed++; Console.WriteLine("PASS " + name);
    }
    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] private static extern nint GetParent(nint window);
    [DllImport("user32.dll")] private static extern nint GetWindowLongPtrW(nint window, int index);
    [DllImport("user32.dll")] private static extern uint GetDpiForWindow(nint window);
}

internal sealed class QuietForm : Form
{
    public QuietForm() { Opacity = .01; ShowInTaskbar = false; StartPosition = FormStartPosition.Manual; Location = new(-10000, -10000); }
    protected override bool ShowWithoutActivation => true;
}
internal sealed class QuietRuntimeForm : RuntimeForm
{
    public QuietRuntimeForm(WindowsDesktop desktop, RuntimeConfig config, bool placeExternal = false) : base(desktop, config, placeExternal)
    { Opacity = 0; ShowInTaskbar = false; }
    protected override bool ShowWithoutActivation => true;
}
