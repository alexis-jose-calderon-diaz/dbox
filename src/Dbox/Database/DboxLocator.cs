namespace Dbox.Database;

public sealed class DboxLocator
{
    public DboxLocation ForInit(string currentDirectory)
    {
        var projectDirectory = NormalizeDirectory(currentDirectory);
        var dboxDirectory = Path.Combine(projectDirectory, ".dbox");
        var databasePath = Path.Combine(dboxDirectory, "data.db");
        return new DboxLocation(projectDirectory, dboxDirectory, databasePath, File.Exists(databasePath));
    }

    public DboxLocation? Find(string startingDirectory)
    {
        var currentDirectory = NormalizeDirectory(startingDirectory);

        while (true)
        {
            var dboxDirectory = Path.Combine(currentDirectory, ".dbox");
            if (Directory.Exists(dboxDirectory))
            {
                var databasePath = Path.Combine(dboxDirectory, "data.db");
                return new DboxLocation(
                    currentDirectory,
                    dboxDirectory,
                    databasePath,
                    File.Exists(databasePath));
            }

            var parent = Directory.GetParent(currentDirectory);
            if (parent is null)
            {
                return null;
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
