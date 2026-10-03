using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Xeneon.Runtime.Windows;

public static class LogFileIdentity
{
    [StructLayout(LayoutKind.Sequential)] private struct FileInfo
    {
        public uint Attributes; public System.Runtime.InteropServices.ComTypes.FILETIME Created, Accessed, Written;
        public uint Volume, SizeHigh, SizeLow, Links, IndexHigh, IndexLow;
    }
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetFileInformationByHandle(SafeFileHandle handle, out FileInfo info);
    // Identity remains private adapter state and is never projected or logged.
    public static string Read(FileStream stream)
    {
        if (!GetFileInformationByHandle(stream.SafeFileHandle, out var info))
            throw new IOException("LOG_IDENTITY_UNAVAILABLE", new Win32Exception(Marshal.GetLastWin32Error()));
        return $"{info.Volume:X8}:{info.IndexHigh:X8}:{info.IndexLow:X8}";
    }
}
