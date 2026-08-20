using Dbox.Database;

namespace Dbox.Tests;

public sealed class DboxLocatorTests
{
    [Fact]
    public void FindsTheNearestProjectDatabase()
    {
        using var project = new TestProject();
        var parentLocation = new DboxLocator().ForInit(project.Root);
        Directory.CreateDirectory(parentLocation.DboxDirectory);
        File.WriteAllText(parentLocation.DatabasePath, string.Empty);

        var child = project.CreateChild();
        var childLocation = new DboxLocator().ForInit(child);
        Directory.CreateDirectory(childLocation.DboxDirectory);
        File.WriteAllText(childLocation.DatabasePath, string.Empty);

        var nested = project.CreateChild("child/nested");
        var result = new DboxLocator().Find(nested);

        Assert.NotNull(result);
        Assert.Equal(childLocation.DatabasePath, result.DatabasePath);
    }

    [Fact]
    public void StopsAtAnIncompleteProjectBoundary()
    {
        using var project = new TestProject();
        var parentLocation = new DboxLocator().ForInit(project.Root);
        Directory.CreateDirectory(parentLocation.DboxDirectory);
        File.WriteAllText(parentLocation.DatabasePath, string.Empty);

        var child = project.CreateChild();
        Directory.CreateDirectory(Path.Combine(child, ".dbox"));

        var result = new DboxLocator().Find(child);

        Assert.NotNull(result);
        Assert.False(result.DatabaseExists);
        Assert.Equal(Path.Combine(child, ".dbox", "data.db"), result.DatabasePath);
    }

    [Fact]
    public void ReturnsNullWhenNoProjectDirectoryExists()
    {
        using var project = new TestProject();

        var result = new DboxLocator().Find(project.Root);

        Assert.Null(result);
    }
}
