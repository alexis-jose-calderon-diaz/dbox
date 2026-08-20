using System.Text.Json;
using Dbox.Cli;

namespace Dbox.Tests;

public sealed class DboxCliTests
{
    [Fact]
    public async Task InitIsIdempotentAndPreservesActivities()
    {
        using var project = new TestProject();

        var firstInit = await TestProject.RunAsync(project.Root, "init");
        var add = await TestProject.RunAsync(project.Root, "activity", "add", "--type", "implementation", "--title", "Keep me");
        var secondInit = await TestProject.RunAsync(project.Root, "init");
        var list = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "json");

        Assert.Equal(0, firstInit.ExitCode);
        Assert.Equal("Database initialized: .dbox/data.db\n", firstInit.Output);
        Assert.Equal(0, add.ExitCode);
        Assert.Equal(0, secondInit.ExitCode);
        Assert.Equal("Database already initialized: .dbox/data.db\n", secondInit.Output);
        Assert.Equal(0, list.ExitCode);
        Assert.Contains("Keep me", list.Output);
        Assert.True(File.Exists(Path.Combine(project.Root, ".dbox", "data.db")));
    }

    [Fact]
    public async Task NestedInitializationUsesAnIndependentDatabase()
    {
        using var project = new TestProject();
        var child = project.CreateChild();

        await TestProject.RunAsync(project.Root, "init");
        await TestProject.RunAsync(project.Root, "activity", "add", "--type", "research", "--title", "Parent");
        var childInit = await TestProject.RunAsync(child, "init");
        var childList = await TestProject.RunAsync(child, "activity", "list", "--output", "json");
        var parentList = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "json");

        Assert.Equal(0, childInit.ExitCode);
        Assert.Equal("[]\n", childList.Output);
        Assert.Contains("Parent", parentList.Output);
    }

    [Fact]
    public async Task ParentDatabaseIsDiscoveredFromNestedDirectory()
    {
        using var project = new TestProject();
        var nested = project.CreateChild("src/feature");

        await TestProject.RunAsync(project.Root, "init");
        await TestProject.RunAsync(project.Root, "activity", "add", "--type", "research", "--title", "Parent activity");
        var result = await TestProject.RunAsync(nested, "activity", "list", "--output", "json");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Parent activity", result.Output);
    }

    [Fact]
    public async Task IncompleteNearestDatabaseBlocksParentDiscovery()
    {
        using var project = new TestProject();
        var child = project.CreateChild();
        Directory.CreateDirectory(Path.Combine(child, ".dbox"));

        await TestProject.RunAsync(project.Root, "init");
        var result = await TestProject.RunAsync(child, "activity", "list");

        Assert.Equal(4, result.ExitCode);
        Assert.Contains("No dbox database found.", result.Error);
        Assert.Contains("Run 'dbox init' to initialize this directory.", result.Error);
    }

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

    [Fact]
    public async Task HelpExposesRootInfrastructureAndActivityCatalog()
    {
        using var project = new TestProject();

        var rootHelp = await TestProject.RunAsync(project.Root, "--help");
        var activityHelp = await TestProject.RunAsync(project.Root, "activity", "--help");

        Assert.Equal(0, rootHelp.ExitCode);
        Assert.Empty(rootHelp.Error);
        Assert.Contains("init", rootHelp.Output);
        Assert.Contains("activity", rootHelp.Output);
        Assert.DoesNotContain("schema", rootHelp.Output);
        Assert.DoesNotContain("add", rootHelp.Output);
        Assert.Equal(0, activityHelp.ExitCode);
        Assert.Empty(activityHelp.Error);
        Assert.Contains("schema", activityHelp.Output);
        Assert.Contains("add", activityHelp.Output);
        Assert.Contains("list", activityHelp.Output);
        Assert.Contains("get", activityHelp.Output);
        Assert.Contains("update", activityHelp.Output);
        Assert.Contains("delete", activityHelp.Output);
    }

    [Fact]
    public async Task FlatActivityCommandsAndRootSchemaAliasAreRejected()
    {
        using var project = new TestProject();
        var flatCommands = new[]
        {
            new[] { "schema" },
            new[] { "add" },
            new[] { "list" },
            new[] { "get", "1" },
            new[] { "update", "1" },
            new[] { "delete", "1" }
        };

        foreach (var arguments in flatCommands)
        {
            var result = await TestProject.RunAsync(project.Root, arguments);

            Assert.Equal(ExitCodes.ValidationError, result.ExitCode);
            Assert.Empty(result.Output);
            Assert.Contains("Validation error:", result.Error);
        }

        var rootSchemaAlias = await TestProject.RunAsync(project.Root, "--schema");
        var rootSchemaAliasJson = await TestProject.RunAsync(project.Root, "--schema", "--json");

        Assert.Equal(ExitCodes.ValidationError, rootSchemaAlias.ExitCode);
        Assert.Equal(ExitCodes.ValidationError, rootSchemaAliasJson.ExitCode);
        Assert.Empty(rootSchemaAlias.Output);
        Assert.Empty(rootSchemaAliasJson.Output);
        Assert.Contains("Validation error:", rootSchemaAlias.Error);
        Assert.Contains("Validation error:", rootSchemaAliasJson.Error);
        Assert.False(Directory.Exists(Path.Combine(project.Root, ".dbox")));
    }

    [Fact]
    public async Task CrudSupportsJsonFiltersPartialUpdatesAndDeletion()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var firstAdd = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--type",
            "research",
            "--title",
            "Investigate",
            "--output",
            "json");
        var secondAdd = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"implementation\",\"title\":\"Build\",\"description\":\"Details\",\"status\":\"pending\"}",
            "--output",
            "json");
        var list = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "json");
        var filteredList = await TestProject.RunAsync(project.Root, "activity", "list", "--type", "implementation", "--status", "pending", "--output", "json");
        var update = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json",
            "{\"description\":null,\"status\":\"completed\"}",
            "--output",
            "json");
        var get = await TestProject.RunAsync(project.Root, "activity", "get", "1", "--output", "json");
        var emptyUpdate = await TestProject.RunAsync(project.Root, "activity", "update", "1", "--output", "json");
        var optionUpdate = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "2",
            "--status",
            "in_progress",
            "--output",
            "json");
        var generatedUpdate = await TestProject.RunAsync(
            project.Root,
            "activity",
            "update",
            "1",
            "--json",
            "{\"id\":99}",
            "--output",
            "json");
        var deleted = await TestProject.RunAsync(project.Root, "activity", "delete", "1", "--output", "json");
        var missing = await TestProject.RunAsync(project.Root, "activity", "get", "1", "--output", "json");
        using var listDocument = JsonDocument.Parse(list.Output);
        using var filteredDocument = JsonDocument.Parse(filteredList.Output);
        using var firstAddDocument = JsonDocument.Parse(firstAdd.Output);
        using var getDocument = JsonDocument.Parse(get.Output);

        Assert.Equal(0, firstAdd.ExitCode);
        Assert.Contains("\"status\": \"completed\"", firstAdd.Output);
        Assert.Matches(
            "^\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}Z$",
            firstAddDocument.RootElement.GetProperty("created_at").GetString());
        Assert.Equal(0, secondAdd.ExitCode);
        Assert.Contains("\"status\": \"pending\"", secondAdd.Output);
        Assert.Equal(0, list.ExitCode);
        Assert.Equal(2, listDocument.RootElement.GetArrayLength());
        Assert.Equal(2, listDocument.RootElement[0].GetProperty("id").GetInt64());
        Assert.Equal(1, listDocument.RootElement[1].GetProperty("id").GetInt64());
        Assert.Equal(1, filteredDocument.RootElement.GetArrayLength());
        Assert.Equal("Build", filteredDocument.RootElement[0].GetProperty("title").GetString());
        Assert.Equal(0, update.ExitCode);
        Assert.Contains("\"description\": null", update.Output);
        Assert.Equal(0, get.ExitCode);
        Assert.Equal("Investigate", getDocument.RootElement.GetProperty("title").GetString());
        Assert.Equal("completed", getDocument.RootElement.GetProperty("status").GetString());
        Assert.True(getDocument.RootElement.GetProperty("description").ValueKind == JsonValueKind.Null);
        Assert.Equal(2, emptyUpdate.ExitCode);
        Assert.Equal(0, optionUpdate.ExitCode);
        Assert.Contains("\"status\": \"in_progress\"", optionUpdate.Output);
        Assert.Equal(2, generatedUpdate.ExitCode);
        Assert.Equal(0, deleted.ExitCode);
        Assert.Contains("\"deleted\": true", deleted.Output);
        Assert.Equal(3, missing.ExitCode);
        Assert.Contains("resource_not_found", missing.Error);
    }

    [Fact]
    public async Task ValidationHappensBeforePersistenceAndUsesStderr()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var invalid = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--type",
            "invalid",
            "--title",
            "",
            "--output",
            "json");
        var list = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "json");

        Assert.Equal(2, invalid.ExitCode);
        Assert.Empty(invalid.Output);
        Assert.Contains("validation_error", invalid.Error);
        Assert.Equal("[]\n", list.Output);
    }

    [Fact]
    public async Task EnumCasingAndTitleLengthAreValidatedBeforePersistence()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var wrongCase = await TestProject.RunAsync(project.Root, "activity", "add", "--type", "Research", "--title", "Valid");
        var tooLong = await TestProject.RunAsync(project.Root, "activity", "add", "--type", "research", "--title", new string('x', 201));
        var list = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "json");

        Assert.Equal(2, wrongCase.ExitCode);
        Assert.Equal(2, tooLong.ExitCode);
        Assert.Contains("must be one of", wrongCase.Error);
        Assert.Contains("at most 200", tooLong.Error);
        Assert.Equal("[]\n", list.Output);
    }

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

    [Fact]
    public async Task TextErrorsUseTheDocumentedMessagesAndExitCodes()
    {
        using var project = new TestProject();

        var missingDatabase = await TestProject.RunAsync(project.Root, "activity", "list");
        await TestProject.RunAsync(project.Root, "init");
        var missingActivity = await TestProject.RunAsync(project.Root, "activity", "get", "7");
        var unsupportedOutput = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "yaml");
        var emptyOutput = await TestProject.RunAsync(project.Root, "activity", "list", "--output=");
        var parserJson = await TestProject.RunAsync(project.Root, "activity", "list", "--output=json", "--unknown");
        var parserJsonAfterUnknown = await TestProject.RunAsync(project.Root, "activity", "list", "--unknown", "--output=json");

        Assert.Equal(4, missingDatabase.ExitCode);
        Assert.Equal("No dbox database found.\nRun 'dbox init' to initialize this directory.\n", missingDatabase.Error);
        Assert.Equal(3, missingActivity.ExitCode);
        Assert.Equal("Activity 7 not found.\n", missingActivity.Error);
        Assert.Equal(2, unsupportedOutput.ExitCode);
        Assert.Contains("Validation error:", unsupportedOutput.Error);
        Assert.Equal(2, emptyOutput.ExitCode);
        Assert.Contains("Validation error:", emptyOutput.Error);
        Assert.Equal(2, parserJson.ExitCode);
        Assert.Empty(parserJson.Output);
        Assert.Contains("\"error\"", parserJson.Error);
        using var parserDocument = JsonDocument.Parse(parserJson.Error);
        Assert.Equal(
            "Invalid command syntax.",
            parserDocument.RootElement.GetProperty("error").GetProperty("details")[0].GetProperty("message").GetString());
        Assert.Equal(2, parserJsonAfterUnknown.ExitCode);
        Assert.Empty(parserJsonAfterUnknown.Output);
        using var parserAfterUnknownDocument = JsonDocument.Parse(parserJsonAfterUnknown.Error);
        Assert.Equal(
            "validation_error",
            parserAfterUnknownDocument.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    [Fact]
    public async Task DatabasePathsWithConnectionStringCharactersRemainUsable()
    {
        using var project = new TestProject();
        var specialDirectory = project.CreateChild("project;Mode=Memory'quoted");

        var init = await TestProject.RunAsync(specialDirectory, "init");
        var list = await TestProject.RunAsync(specialDirectory, "activity", "list", "--output", "json");

        Assert.Equal(0, init.ExitCode);
        Assert.Equal(0, list.ExitCode);
        Assert.Equal("[]\n", list.Output);
    }

    [Fact]
    public async Task TextOutputEscapesControlCharactersInActivityValues()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var add = await TestProject.RunAsync(project.Root, "activity", "add", "--type", "research", "--title", "line\nbreak\tvalue");
        var get = await TestProject.RunAsync(project.Root, "activity", "get", "1");

        Assert.Equal(0, add.ExitCode);
        Assert.Equal(0, get.ExitCode);
        Assert.Contains("TITLE: line\\nbreak\\tvalue", get.Output);
        Assert.DoesNotContain("TITLE: line\nbreak", get.Output);
    }

    [Fact]
    public async Task ExistingEmptyDatabaseIsMigratedBeforeACommand()
    {
        using var project = new TestProject();
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        Directory.CreateDirectory(databaseDirectory);
        File.WriteAllBytes(Path.Combine(databaseDirectory, "data.db"), []);

        var list = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "json");

        Assert.Equal(0, list.ExitCode);
        Assert.Equal("[]\n", list.Output);
    }

    [Fact]
    public async Task InitReportsMigrationForAnExistingDatabaseWithPendingMigrations()
    {
        using var project = new TestProject();
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        Directory.CreateDirectory(databaseDirectory);
        File.WriteAllBytes(Path.Combine(databaseDirectory, "data.db"), []);

        var init = await TestProject.RunAsync(project.Root, "init");

        Assert.Equal(0, init.ExitCode);
        Assert.Equal("Database migrated: .dbox/data.db\n", init.Output);
    }

    [Fact]
    public async Task InvalidDatabaseReturnsDatabaseErrorWithoutOutput()
    {
        using var project = new TestProject();
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        Directory.CreateDirectory(databaseDirectory);
        File.WriteAllText(Path.Combine(databaseDirectory, "data.db"), "not a sqlite database");

        var result = await TestProject.RunAsync(project.Root, "activity", "list", "--output", "json");
        var init = await TestProject.RunAsync(project.Root, "init", "--output", "json");

        Assert.Equal(4, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("database_error", result.Error);
        Assert.Equal(4, init.ExitCode);
        Assert.Empty(init.Output);
        Assert.Contains("database_error", init.Error);
    }

    [Fact]
    public async Task MixedInputAndUnknownJsonPropertiesAreRejected()
    {
        using var project = new TestProject();
        await TestProject.RunAsync(project.Root, "init");

        var mixed = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"research\",\"title\":\"Test\"}",
            "--type",
            "research");
        var unknown = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"research\",\"title\":\"Test\",\"unknown\":true}");

        Assert.Equal(2, mixed.ExitCode);
        Assert.Equal(2, unknown.ExitCode);
        Assert.Contains("Validation error:", mixed.Error);
        Assert.Contains("Unknown property", unknown.Error);
    }
}
