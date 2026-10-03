using System.Text.Json;
using Xeneon.Runtime.Core;
using Xeneon.Runtime.Windows;

namespace Xeneon.Runtime.Host;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length == 1 && args[0] == "--shutdown")
            {
                // A shared dotnet.exe host must never be treated as our installation.
                string? executable = Environment.ProcessPath;
                if (!string.Equals(Path.GetFileNameWithoutExtension(executable), typeof(Program).Assembly.GetName().Name, StringComparison.OrdinalIgnoreCase)) return 3;
                return ApplicationLifetime.CloseInstallation(executable!) ? 0 : 3;
            }
            string? configPath = null, inspectProcess = null; bool probe = false, placeExternal = false, configure = false;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--config":
                        if (configPath is not null || i + 1 >= args.Length || args[i + 1].StartsWith("--")) throw new ArgumentException("Invalid config argument.");
                        configPath = args[++i]; break;
                    case "--probe": probe = true; break;
                    case "--configure": configure = true; break;
                    case "--inspect-process":
                        if (inspectProcess is not null || i + 1 >= args.Length || args[i + 1].StartsWith("--")) throw new ArgumentException("Invalid inspection argument.");
                        inspectProcess = args[++i]; ExternalSelector.ValidateProcessName(inspectProcess); break;
                    case "--place-external": placeExternal = true; break;
                    default: throw new ArgumentException("Unknown argument.");
                }
            }
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            bool opensWindow = !probe && inspectProcess is null;
            using var runningMarker = opensWindow ? ApplicationLifetime.TryAcquire() : null;
            if (opensWindow && runningMarker is null)
            {
                MessageBox.Show("Das Begleiter-Deck läuft bereits. Bitte die laufende Sitzung zuerst beenden.", "Begleiter-Deck bereits geöffnet");
                return 0;
            }
            bool normalStart = args.Length == 0 || configure;
            if (configure && args.Length != 1) throw new ArgumentException("--configure muss allein verwendet werden.");
            RuntimeConfig config;
            if (normalStart)
            {
                RuntimeConfig? stored = null;
                if (File.Exists(SetupForm.ConfigPath))
                {
                    try { stored = RuntimeConfig.Load(SetupForm.ConfigPath); }
                    catch (Exception e) when (e is ArgumentException or JsonException or IOException or UnauthorizedAccessException)
                    { MessageBox.Show("Die gespeicherte Konfiguration konnte nicht gelesen werden. Bitte erneut einrichten.", "Begleiter-Deck"); }
                }
                if (configure || stored is null)
                {
                    using var setup = new SetupForm(stored);
                    if (setup.ShowDialog() != DialogResult.OK || setup.Result is null) return 0;
                    config = setup.Result;
                }
                else config = stored;
                placeExternal = true;
            }
            else config = configPath is null ? new RuntimeConfig() : RuntimeConfig.Load(configPath);
            config.Validate();
            var desktop = new WindowsDesktop();
            if (inspectProcess is not null)
            {
                if (probe || placeExternal) throw new ArgumentException("Inspection is a standalone read-only command.");
                var windows = desktop.InspectWindows(inspectProcess).Select(w => new
                { w.ProcessName, w.ClassName, w.Title, w.Bounds, w.Minimized });
                Console.WriteLine(JsonSerializer.Serialize(windows, RuntimeConfig.Json));
                return 0;
            }
            if (probe)
            {
                using var session = new RuntimeSession(desktop, config);
                // Discovery only: no own window is opened and no third-party window is moved.
                var snapshot = session.Refresh();
                Console.WriteLine(JsonSerializer.Serialize(snapshot, RuntimeConfig.Json));
                return snapshot.State == HealthState.ERROR ? 2 : 0;
            }
            Application.EnableVisualStyles();
            using var form = new RuntimeForm(desktop, config, placeExternal);
            Application.Run(form);
            return 0;
        }
        catch (Exception e) when (e is not OutOfMemoryException)
        {
            Console.Error.WriteLine("RUNTIME_START_FAILED: " + e.GetType().Name);
            return 2;
        }
    }
}
