namespace Dbox.Database;

public sealed class DboxLocator
{
    public DboxLocation ForInit(string currentDirectory)
    {
        var projectDirectory = NormalizeDirectory(currentDirectory);
        var dboxDirectory = Path.Combine(projectDirectory, ".dbox");
        var databasePath = Path.Combine(dboxDirectory, "data.db");
        return new DboxLocation(
            projectDirectory,
            projectDirectory,
            dboxDirectory,
            databasePath,
            File.Exists(databasePath)
                ? DboxDiscoveryStatus.Found
                : Directory.Exists(dboxDirectory)
                    ? DboxDiscoveryStatus.Incomplete
                    : DboxDiscoveryStatus.NotFound);
    }

    public DboxLocation Find(string startingDirectory)
    {
        var currentDirectory = NormalizeDirectory(startingDirectory);
        var cwd = currentDirectory;

        while (true)
        {
            var dboxDirectory = Path.Combine(currentDirectory, ".dbox");
            if (Directory.Exists(dboxDirectory))
            {
                var databasePath = Path.Combine(dboxDirectory, "data.db");
                return new DboxLocation(
                    cwd,
                    currentDirectory,
                    dboxDirectory,
                    databasePath,
                    File.Exists(databasePath)
                        ? DboxDiscoveryStatus.Found
                        : DboxDiscoveryStatus.Incomplete);
            }

            var parent = Directory.GetParent(currentDirectory);
            if (parent is null)
            {
                return new DboxLocation(cwd, null, null, null, DboxDiscoveryStatus.NotFound);
            }

            currentDirectory = parent.FullName;
        }
    }

    private static string NormalizeDirectory(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("A starting directory is required.", nameof(directory));
        }

        return Path.GetFullPath(directory);
    }
}
