namespace Dbox.Database;

public static class DboxFilePermissions
{
    public static void HardenDirectory(string dboxDirectory)
    {
        if (OperatingSystem.IsLinux())
        {
            File.SetUnixFileMode(
                dboxDirectory,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }

    public static void HardenDatabase(string databasePath)
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var privateFileMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        File.SetUnixFileMode(databasePath, privateFileMode);
        foreach (var suffix in new[] { "-wal", "-shm", "-journal" })
        {
            var sidecarPath = databasePath + suffix;
            if (File.Exists(sidecarPath))
            {
                File.SetUnixFileMode(sidecarPath, privateFileMode);
            }
        }
    }
}
