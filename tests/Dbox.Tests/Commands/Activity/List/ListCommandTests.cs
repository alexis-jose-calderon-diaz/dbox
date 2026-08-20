using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Activity.List;

public sealed class ListCommandTests
{
    [Fact]
    public async Task InitJsonAndTextListUseTheDocumentedOutput()
    {
        using var project = new TestProject();

        var init = await TestProject.RunAsync(project.Root, "init", "--output", "json");
        var list = await TestProject.RunAsync(project.Root, "activity", "list");
        using var initDocument = JsonDocument.Parse(init.Output);

        Assert.Equal(0, init.ExitCode);
        Assert.Equal("initialized", initDocument.RootElement.GetProperty("status").GetString());
        Assert.Equal(".dbox/data.db", initDocument.RootElement.GetProperty("database").GetString());
        Assert.Equal(0, list.ExitCode);
        Assert.Contains("ID", list.Output);
        Assert.Contains("CREATED_AT", list.Output);
        Assert.Contains("TYPE", list.Output);
        Assert.Contains("STATUS", list.Output);
        Assert.Contains("TITLE", list.Output);
    }
}
