using System.Text.Json;
using System.Diagnostics;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Xeneon.Runtime.Core;
using Xeneon.Runtime.Windows;

namespace Xeneon.Runtime.Host;

internal class RuntimeForm : Form
{
    private const string Origin = "https://xee-runtime.local";
    private readonly WindowsDesktop desktop;
    private readonly RuntimeConfig config;
    private readonly RuntimeSession session;
    private readonly System.Windows.Forms.Timer refresh;
    private readonly WebView2 web = new() { Dock = DockStyle.Fill };
    private readonly Label fallback = new() { Dock = DockStyle.Fill, Text = "Anwendung startet…", TextAlign = ContentAlignment.MiddleCenter };
    private readonly DiagnosticLog log = new();
    private readonly GameLogTailer? gameLog;
    private GameLogTailer? orionEvents;
    private string companionName;
    private bool placeExternal;
    private bool webReady;
    private bool closing;
    private bool allowClose;
    private string? previousHealth;
    private RuntimeSnapshot? latest;
    internal RuntimeSnapshot CurrentState => latest ?? session.Snapshot;
    internal WebView2 WebView => web;
    internal bool WebReady => webReady;

    public RuntimeForm(WindowsDesktop desktop, RuntimeConfig config, bool placeExternal)
    {
        this.desktop = desktop; this.config = config; this.placeExternal = placeExternal;
        companionName = config.CompanionVariant;
        session = new(desktop, config);
        if (config.GameLogPath is not null) gameLog = new(config.GameLogPath, LogFileIdentity.Read);
        OpenCompanionFeed();
        Text = "Star Citizen Begleiter-Deck"; Width = 1100; Height = 420;
        StartPosition = FormStartPosition.CenterScreen; KeyPreview = true;
        Controls.Add(web); Controls.Add(fallback); fallback.BringToFront();
        refresh = new() { Interval = config.RefreshMilliseconds };
        refresh.Tick += (_, _) => RefreshState();
        Shown += async (_, _) => await InitializeWeb();
        KeyDown += HandleShortcut;
        FormClosing += RestoreBeforeClose;
        DpiChanged += (_, _) => { if (!closing && IsHandleCreated) BeginInvoke((Action)RefreshState); };
    }

    private void OpenCompanionFeed()
    {
        orionEvents?.Dispose(); orionEvents = null;
        if (config.GameLogPath is null && config.CompanionEventsPath is null && config.OrionEventsPath is null) return;
        string path = companionName == config.CompanionVariant
            ? config.CompanionEventsPath ?? config.OrionEventsPath ?? DefaultEventsPath(companionName)
            : DefaultEventsPath(companionName);
        string variant = companionName;
        orionEvents = new(path, LogFileIdentity.Read,
            (line, sequence, generation, historical) => CompanionEventParser.Parse(line, sequence, generation, historical, variant));
    }

    private static string DefaultEventsPath(string variant) => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VoiceAttack", variant + " Log-Wächter", variant + "-Companion-Events.jsonl");

    private void SelectCompanion(string variant)
    {
        if (variant == companionName) return;
        var selector = variant == config.CompanionVariant && config.ExternalWindow is not null
            ? config.ExternalWindow : new ExternalSelector(variant + " Log-Wächter", Title: variant == "Aurora" ? "Aurora Orb" : "Orion Companion");
        if (!session.SelectExternal(selector))
        { log.Write("DEGRADED", "COMPANION_SWITCH_RESTORE_PENDING"); RefreshState(); return; }
        companionName = variant;
        OpenCompanionFeed();
        RefreshState();
    }

    private async Task InitializeWeb()
    {
        try
        {
            var environment = await CoreWebView2Environment.CreateAsync(null,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "XeneonEdge", "WebView2"));
            if (closing) return;
            await web.EnsureCoreWebView2Async(environment);
            if (closing) return;
            var core = web.CoreWebView2;
            core.Settings.AreDevToolsEnabled = false;
            core.Settings.AreDefaultContextMenusEnabled = false;
            core.Settings.IsStatusBarEnabled = false;
            core.Settings.AreBrowserAcceleratorKeysEnabled = false;
            core.Settings.IsGeneralAutofillEnabled = false;
            core.Settings.IsPasswordAutosaveEnabled = false;
            // WinForms WebView2 forwards accelerator keys to its standard key events.
            web.KeyDown += HandleShortcut;
            core.SetVirtualHostNameToFolderMapping("xee-runtime.local", Path.Combine(AppContext.BaseDirectory, "web-ui"), CoreWebView2HostResourceAccessKind.DenyCors);
            core.NavigationStarting += (_, e) => e.Cancel = e.Uri != Origin + "/index.html";
            core.NewWindowRequested += (_, e) => e.Handled = true;
            core.DownloadStarting += (_, e) => e.Cancel = true;
            core.PermissionRequested += (_, e) => e.State = CoreWebView2PermissionState.Deny;
            core.WebResourceRequested += (_, e) =>
            {
                if (!e.Request.Uri.StartsWith(Origin + "/", StringComparison.Ordinal))
                    e.Response = core.Environment.CreateWebResourceResponse(null, 403, "Blocked", "");
            };
            core.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            core.WebMessageReceived += (_, e) =>
            {
                if (e.Source != Origin + "/index.html") return;
                string command;
                try { command = e.TryGetWebMessageAsString(); } catch (ArgumentException) { return; }
                switch (command)
                {
                    case "refresh": RefreshState(); break;
                    case "toggle-mode": ToggleMode(); break;
                    case "release-external":
                        placeExternal = false;
                        foreach (var result in session.ReleaseExternal()) log.Write(result.State.ToString(), result.Code);
                        RefreshState(); break;
                    case "close": Close(); break;
                    case "companion:Orion": SelectCompanion("Orion"); break;
                    case "companion:Aurora": SelectCompanion("Aurora"); break;
                }
            };
            core.NavigationCompleted += (_, e) =>
            {
                webReady = e.IsSuccess;
                fallback.Visible = !webReady;
                if (!webReady) fallback.Text = "Oberfläche nicht verfügbar. Escape schließt die Anwendung.";
                RefreshState();
            };
            core.ProcessFailed += (_, _) =>
            {
                webReady = false; fallback.Visible = true; fallback.Text = "Oberfläche angehalten. Anwendung schließen und erneut starten.";
                log.Write("ERROR", "WEB_PROCESS_FAILED");
            };
            core.Navigate(Origin + "/index.html");
        }
        catch (Exception e) when (e is not OutOfMemoryException)
        {
            fallback.Text = "WebView2 nicht verfügbar. Microsoft Edge WebView2 Runtime installieren und erneut starten. Escape schließt die Anwendung.";
            log.Write("ERROR", "WEB_INITIALIZATION_FAILED");
        }
        if (!closing) { RefreshState(); refresh.Start(); }
    }

    private void HandleShortcut(object? sender, KeyEventArgs e)
    {
        if (closing || e.Handled || !IsHandleCreated) return;
        if (e.KeyCode == Keys.Escape)
        { e.SuppressKeyPress = true; BeginInvoke((Action)Close); }
        else if (e.KeyCode == Keys.F11)
        { e.SuppressKeyPress = true; BeginInvoke((Action)ToggleMode); }
    }

    private async void RestoreBeforeClose(object? sender, FormClosingEventArgs e)
    {
        if (allowClose) return;
        e.Cancel = true;
        if (closing) return;
        closing = true; refresh.Stop();
        var deadline = Stopwatch.StartNew();
        do
        {
            foreach (var result in session.ReleaseExternal()) log.Write(result.State.ToString(), result.Code);
            if (!session.HasPendingRestoration || deadline.ElapsedMilliseconds >= 750) break;
            // Allow the external process and our UI queue to process the asynchronous position request.
            await Task.Delay(50);
        } while (!IsDisposed);
        if (IsDisposed) return;
        allowClose = true;
        // Schedule the accepted close to avoid re-entering FormClosing synchronously.
        BeginInvoke((Action)Close);
    }

    private void ToggleMode()
    {
        session.Mode = session.Mode == WindowMode.Fullscreen ? WindowMode.Windowed : WindowMode.Fullscreen;
        RefreshState();
    }

    private PlacementResult PlaceHost(DisplayInfo display, WindowMode mode)
    {
        // Native SetWindowPos owns the outer rectangle. WinForms logical coordinates are not used for placement.
        FormBorderStyle = mode == WindowMode.Fullscreen ? FormBorderStyle.None : FormBorderStyle.Sizable;
        WindowState = FormWindowState.Normal;
        var identity = desktop.ReadWindow(Handle);
        if (identity is null) return new(false, "HOST_WINDOW_MISSING");
        var bounds = mode == WindowMode.Fullscreen ? display.Bounds : config.WindowedArea.On(display.WorkArea);
        return desktop.Place(identity, bounds);
    }

    private void RefreshState()
    {
        if (closing || IsDisposed) return;
        var state = session.Refresh(PlaceHost, placeExternal && webReady);
        if (gameLog is not null)
        {
            var feed = gameLog.Poll();
            state = state with { StarCitizen = feed,
                State = state.State == HealthState.READY && feed.State != HealthState.READY ? HealthState.DEGRADED : state.State,
                Diagnostics = [.. state.Diagnostics, new("gameLog", feed.State, feed.Code)] };
        }
        if (orionEvents is not null)
        {
            var feed = orionEvents.Poll();
            state = state with { CompanionEvents = feed, CompanionName = companionName,
                State = state.State == HealthState.READY && feed.State != HealthState.READY ? HealthState.DEGRADED : state.State,
                Diagnostics = [.. state.Diagnostics, new("companionEvents", feed.State, feed.Code)] };
        }
        // Loss/ambiguity of the target must not leave an inaccessible borderless host off-screen.
        if (state.Display is null && (latest?.Display is not null || FormBorderStyle == FormBorderStyle.None))
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            var safe = Screen.PrimaryScreen?.WorkingArea;
            if (safe is not null)
            {
                var identity = desktop.ReadWindow(Handle);
                if (identity is not null)
                {
                    var rectangle = RecoveryBounds(new(safe.Value.X, safe.Value.Y, safe.Value.Width, safe.Value.Height));
                    var recovered = desktop.Place(identity, rectangle);
                    state = state with { Diagnostics = [.. state.Diagnostics,
                        new("host", recovered.Confirmed ? HealthState.READY : HealthState.DEGRADED,
                            recovered.Confirmed ? "HOST_RECOVERED" : "HOST_RECOVERY_UNCONFIRMED")] };
                }
            }
        }
        if (!webReady) state = state with { State = HealthState.ERROR,
            Diagnostics = [.. state.Diagnostics, new("webUi", HealthState.ERROR, "WEB_UNAVAILABLE")] };
        state = state with { CompanionName = companionName };
        latest = state;
        var signature = state.State + ":" + string.Join(',', state.Diagnostics.Select(d => d.Code));
        if (signature != previousHealth) { log.Write(state.State.ToString(), string.Join(',', state.Diagnostics.Select(d => d.Code))); previousHealth = signature; }
        if (webReady) web.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(state, RuntimeConfig.Json));
    }

    internal static PixelRect RecoveryBounds(PixelRect workArea)
    {
        int marginX = Math.Min(20, Math.Max(0, (workArea.Width - 1) / 2));
        int marginY = Math.Min(20, Math.Max(0, (workArea.Height - 1) / 2));
        return new(workArea.X + marginX, workArea.Y + marginY,
            Math.Max(1, Math.Min(1100, workArea.Width - marginX * 2)),
            Math.Max(1, Math.Min(420, workArea.Height - marginY * 2)));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { closing = true; refresh.Dispose(); session.Dispose(); gameLog?.Dispose(); orionEvents?.Dispose(); web.Dispose(); log.Dispose(); }
        base.Dispose(disposing);
    }
}
