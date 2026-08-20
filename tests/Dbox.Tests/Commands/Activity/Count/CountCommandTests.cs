using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Activity.Count;

public sealed class CountCommandTests
{
    [Fact]
    public async Task CountSupportsOptionalFiltersAndListPaginatesInCreationOrder()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");
        await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"research\",\"title\":\"First\",\"status\":\"pending\"}");
        await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"implementation\",\"title\":\"Second\"}");
        await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"research\",\"title\":\"Third\",\"status\":\"pending\"}");

        var count = await TestProject.RunAsync(project.Root, "activity", "count");
        var filteredCount = await TestProject.RunAsync(project.Root, "activity", "count", "--json", "{\"type\":\"research\",\"status\":\"pending\"}");
        var page = await TestProject.RunAsync(project.Root, "activity", "list", "--skip", "1", "--take", "1");
        var invalidFilter = await TestProject.RunAsync(project.Root, "activity", "count", "--json", "{\"unknown\":true}");
        var invalidPagination = await TestProject.RunAsync(project.Root, "activity", "list", "--skip", "-1");
        using var countDocument = JsonDocument.Parse(count.Output);
        using var filteredCountDocument = JsonDocument.Parse(filteredCount.Output);
        using var pageDocument = JsonDocument.Parse(page.Output);

        Assert.Equal(0, count.ExitCode);
        Assert.Equal(3, countDocument.RootElement.GetProperty("count").GetInt32());
        Assert.Equal(0, filteredCount.ExitCode);
        Assert.Equal(2, filteredCountDocument.RootElement.GetProperty("count").GetInt32());
        Assert.Equal(0, page.ExitCode);
        Assert.Equal("Second", pageDocument.RootElement[0].GetProperty("title").GetString());
        Assert.Equal(2, invalidFilter.ExitCode);
        Assert.Equal(2, invalidPagination.ExitCode);
        Assert.Contains("validation_error", invalidFilter.Error);
        Assert.Contains("validation_error", invalidPagination.Error);
    }
}
