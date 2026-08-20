using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Activity.Schema;

public sealed class SchemaCommandTests
{
    [Fact]
    public async Task SchemaJsonUsesTheStableContractAndAlias()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var schema = await TestProject.RunAsync(project.Root, "activity", "schema", "--json");
        var alias = await TestProject.RunAsync(project.Root, "activity", "--schema", "--json");
        var aliasWithGlobalOutput = await TestProject.RunAsync(project.Root, "--output=json", "activity", "--schema", "--json");
        var humanSchema = await TestProject.RunAsync(project.Root, "activity", "schema");
        using var document = JsonDocument.Parse(schema.Output);
        var fields = document.RootElement.GetProperty("entities").GetProperty("activity").GetProperty("fields");

        Assert.Equal(0, schema.ExitCode);
        Assert.Equal(0, alias.ExitCode);
        Assert.Equal(0, aliasWithGlobalOutput.ExitCode);
        Assert.Equal(0, humanSchema.ExitCode);
        Assert.Equal(schema.Output, alias.Output);
        Assert.Equal(schema.Output, aliasWithGlobalOutput.Output);
        Assert.Contains("non-blank", humanSchema.Output);
        Assert.Contains("optional, nullable", humanSchema.Output);
        Assert.Equal(200, fields.GetProperty("title").GetProperty("maxLength").GetInt32());
        Assert.False(fields.GetProperty("id").TryGetProperty("required", out _));
        Assert.True(fields.GetProperty("id").GetProperty("generated").GetBoolean());
        Assert.False(fields.GetProperty("id").GetProperty("mutable").GetBoolean());
        Assert.True(fields.GetProperty("created_at").GetProperty("generated").GetBoolean());
        Assert.Equal("completed", fields.GetProperty("status").GetProperty("default").GetString());
        Assert.Equal("research", fields.GetProperty("type").GetProperty("enum")[0].GetString());
    }
}
