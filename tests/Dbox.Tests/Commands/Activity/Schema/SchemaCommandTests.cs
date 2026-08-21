using System.Text.Json;
using Dbox.Tests.Support;

namespace Dbox.Tests.Commands.Activity.Schema;

public sealed class SchemaCommandTests
{
    [Fact]
    public async Task SchemaUsesTheStableJsonContractWithoutAliases()
    {
        using var project = new TestProject();

        var schema = await TestProject.RunAsync(project.Root, "activity", "schema");
        var alias = await TestProject.RunAsync(project.Root, "activity", "--schema");
        var formatOption = await TestProject.RunAsync(project.Root, "activity", "schema", "--json");
        using var document = JsonDocument.Parse(schema.Output);
        var fields = document.RootElement.GetProperty("entities").GetProperty("activity").GetProperty("fields");

        Assert.Equal(0, schema.ExitCode);
        Assert.Equal(2, alias.ExitCode);
        Assert.Equal(2, formatOption.ExitCode);
        Assert.Empty(alias.Output);
        Assert.Empty(formatOption.Output);
        Assert.Contains("validation_error", alias.Error);
        Assert.Contains("validation_error", formatOption.Error);
        Assert.Equal(13, fields.EnumerateObject().Count());
        Assert.Equal("id", fields.GetProperty("id").GetProperty("name").GetString());
        Assert.Equal("integer", fields.GetProperty("id").GetProperty("type").GetString());
        Assert.Equal(200, fields.GetProperty("title").GetProperty("maxLength").GetInt32());
        Assert.False(fields.GetProperty("id").GetProperty("required").GetBoolean());
        Assert.True(fields.GetProperty("id").GetProperty("generated").GetBoolean());
        Assert.False(fields.GetProperty("id").GetProperty("mutable").GetBoolean());
        Assert.False(fields.GetProperty("created_at").GetProperty("required").GetBoolean());
        Assert.True(fields.GetProperty("created_at").GetProperty("generated").GetBoolean());
        Assert.True(fields.GetProperty("description").GetProperty("required").GetBoolean());
        Assert.Equal("Descripcion de lo realizado.", fields.GetProperty("description").GetProperty("description").GetString());
        Assert.False(fields.GetProperty("type").TryGetProperty("enum", out _));
        Assert.Equal("pending", fields.GetProperty("status").GetProperty("enum")[0].GetString());
        Assert.False(fields.GetProperty("status").TryGetProperty("default", out _));
        Assert.Equal("low", fields.GetProperty("effort").GetProperty("enum")[0].GetString());
        Assert.Equal("very-high", fields.GetProperty("effort").GetProperty("enum")[3].GetString());
        Assert.Equal("json", fields.GetProperty("metadata").GetProperty("type").GetString());
        Assert.False(fields.GetProperty("metadata").GetProperty("required").GetBoolean());
        Assert.True(fields.GetProperty("metadata").GetProperty("nullable").GetBoolean());
        Assert.False(Directory.Exists(Path.Combine(project.Root, ".dbox")));
    }

    [Fact]
    public async Task SchemaDoesNotMigrateAnExistingDatabase()
    {
        using var project = new TestProject();
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        var databasePath = Path.Combine(databaseDirectory, "data.db");
        Directory.CreateDirectory(databaseDirectory);
        File.WriteAllBytes(databasePath, []);
        var before = File.ReadAllBytes(databasePath);

        var schema = await TestProject.RunAsync(project.Root, "activity", "schema");

        Assert.Equal(0, schema.ExitCode);
        Assert.Equal(before, File.ReadAllBytes(databasePath));
    }
}
