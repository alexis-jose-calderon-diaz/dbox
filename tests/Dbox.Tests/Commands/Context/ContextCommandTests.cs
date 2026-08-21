using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Context;

public sealed class ContextCommandTests
{
    [Fact]
    public async Task ContextReportsFoundDatabaseAndAbsolutePaths()
    {
        using var project = new TestProject();
        var nested = project.CreateChild("src/feature");
        await TestProject.RunAsync(project.Root, "init");

        var result = await TestProject.RunAsync(nested, "context");
        using var document = JsonDocument.Parse(result.Output);

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Equal("found", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(Path.GetFullPath(nested), document.RootElement.GetProperty("cwd").GetString());
        Assert.Equal(Path.GetFullPath(project.Root), document.RootElement.GetProperty("project_directory").GetString());
        Assert.Equal(Path.Combine(Path.GetFullPath(project.Root), ".dbox"), document.RootElement.GetProperty("dbox_directory").GetString());
        Assert.Equal(Path.Combine(Path.GetFullPath(project.Root), ".dbox", "data.db"), document.RootElement.GetProperty("database").GetString());
    }

    [Fact]
    public async Task ContextReportsIncompleteNearestBoundaryWithoutSearchingAncestors()
    {
        using var project = new TestProject();
        var child = project.CreateChild();
        await TestProject.RunAsync(project.Root, "init");
        var childDbox = Path.Combine(child, ".dbox");
        Directory.CreateDirectory(childDbox);

        var result = await TestProject.RunAsync(child, "context");
        using var document = JsonDocument.Parse(result.Output);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("incomplete", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(Path.GetFullPath(child), document.RootElement.GetProperty("project_directory").GetString());
        Assert.Equal(Path.GetFullPath(childDbox), document.RootElement.GetProperty("dbox_directory").GetString());
        Assert.Equal(Path.Combine(Path.GetFullPath(childDbox), "data.db"), document.RootElement.GetProperty("database").GetString());
    }

    [Fact]
    public async Task ContextReportsNotFoundWithoutCreatingProjectFiles()
    {
        using var project = new TestProject();

        var result = await TestProject.RunAsync(project.Root, "context");
        using var document = JsonDocument.Parse(result.Output);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal("not_found", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(Path.GetFullPath(project.Root), document.RootElement.GetProperty("cwd").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("project_directory").ValueKind);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("dbox_directory").ValueKind);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("database").ValueKind);
        Assert.False(Directory.Exists(Path.Combine(project.Root, ".dbox")));
    }
}
