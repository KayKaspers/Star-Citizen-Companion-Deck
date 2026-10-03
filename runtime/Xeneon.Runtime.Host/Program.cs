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
            string? configPath = null, inspectProcess = null; bool probe = false, placeExternal = false;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--config":
                        if (configPath is not null || i + 1 >= args.Length || args[i + 1].StartsWith("--")) throw new ArgumentException("Invalid config argument.");
                        configPath = args[++i]; break;
                    case "--probe": probe = true; break;
                    case "--inspect-process":
                        if (inspectProcess is not null || i + 1 >= args.Length || args[i + 1].StartsWith("--")) throw new ArgumentException("Invalid inspection argument.");
                        inspectProcess = args[++i]; ExternalSelector.ValidateProcessName(inspectProcess); break;
                    case "--place-external": placeExternal = true; break;
                    default: throw new ArgumentException("Unknown argument.");
                }
            }
            var config = configPath is null ? new RuntimeConfig() : RuntimeConfig.Load(configPath);
            config.Validate();
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
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
