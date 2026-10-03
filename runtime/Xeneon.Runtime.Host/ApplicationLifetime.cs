using System.ComponentModel;
using System.Diagnostics;
using Xeneon.Runtime.Windows;

namespace Xeneon.Runtime.Host;

internal static class ApplicationLifetime
{
    internal const string MutexName = @"Local\StarCitizenCompanionDeck.Running";
    internal static Mutex? TryAcquire()
    {
        var marker = new Mutex(false, MutexName, out bool created);
        if (created) return marker;
        marker.Dispose(); return null;
    }

    // Close only this installation in this session, never kill a process.
    internal static bool CloseInstallation(string executablePath, int timeoutMilliseconds = 15000)
    {
        executablePath = Path.GetFullPath(executablePath);
        using var self = Process.GetCurrentProcess();
        string name = Path.GetFileNameWithoutExtension(executablePath);
        var timer = Stopwatch.StartNew();
        var desktop = new WindowsDesktop();
        while (true)
        {
            bool pending = false;
            foreach (var candidate in Process.GetProcessesByName(name))
            {
                using (candidate)
                {
                    try
                    {
                        if (candidate.Id == self.Id || candidate.SessionId != self.SessionId || candidate.HasExited) continue;
                        if (!string.Equals(candidate.MainModule?.FileName, executablePath, StringComparison.OrdinalIgnoreCase)) continue;
                        pending = true;
                        foreach (var window in desktop.InspectWindows(name).Where(w => w.ProcessId == candidate.Id))
                            desktop.RequestClose(window);
                    }
                    catch (InvalidOperationException) { } // Process exited during observation.
                    catch (Win32Exception) { return false; } // Cannot confirm identity/close; do not remove files.
                }
            }
            if (!pending) return true;
            if (timer.ElapsedMilliseconds >= timeoutMilliseconds) return false;
            Thread.Sleep(50);
        }
    }
}
