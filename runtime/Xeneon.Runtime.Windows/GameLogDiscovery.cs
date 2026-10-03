namespace Xeneon.Runtime.Windows;

public static class GameLogDiscovery
{
    // Bounded probes only; no recursive scan of disks or network shares.
    private static readonly string[] Folders = ["", "Games", "Spiele", "Program Files", "Program Files (x86)"];
    public static IReadOnlyList<string> Find(IEnumerable<string> roots, Func<string, bool>? exists = null)
    {
        exists ??= File.Exists;
        var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string root in roots.Take(26))
        {
            if (!Path.IsPathFullyQualified(root) || root.StartsWith(@"\\", StringComparison.Ordinal)) continue;
            foreach (string folder in Folders)
            foreach (string installation in new[] { Path.Combine("Roberts Space Industries", "StarCitizen"), "StarCitizen" })
            {
                string candidate = Path.Combine(root, folder, installation, "LIVE", "Game.log");
                try { if (exists(candidate)) found.Add(candidate); }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
            }
        }
        return found.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
    }
    public static IReadOnlyList<string> FindLocal() => Find(DriveInfo.GetDrives()
        .Where(d => d.DriveType == DriveType.Fixed).Select(d => d.RootDirectory.FullName));
}
