using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Activity.List;

public sealed class ListCommandTests
{
    [Fact]
    public async Task InitAndListUseJsonOutputByDefault()
    {
        using var project = new TestProject();

        var init = await TestProject.RunAsync(project.Root, "init");
        var list = await TestProject.RunAsync(project.Root, "activity", "list");
        using var initDocument = JsonDocument.Parse(init.Output);
        using var listDocument = JsonDocument.Parse(list.Output);

        Assert.Equal(0, init.ExitCode);
        Assert.Equal("initialized", initDocument.RootElement.GetProperty("status").GetString());
        Assert.Equal(".dbox/data.db", initDocument.RootElement.GetProperty("database").GetString());
        Assert.Equal(0, list.ExitCode);
        Assert.Empty(listDocument.RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal(0, listDocument.RootElement.GetProperty("pagination").GetProperty("skip").GetInt32());
        Assert.Equal(100, listDocument.RootElement.GetProperty("pagination").GetProperty("take").GetInt32());
        Assert.Equal(0, listDocument.RootElement.GetProperty("pagination").GetProperty("total").GetInt32());
        Assert.False(listDocument.RootElement.GetProperty("pagination").GetProperty("has_more").GetBoolean());
    }
}
