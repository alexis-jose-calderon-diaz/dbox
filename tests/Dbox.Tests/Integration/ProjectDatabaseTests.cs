using System.Data.Common;
using System.Text.Json;
using Dbox.Database;
using Dbox.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace Dbox.Tests.Integration;

public sealed class ProjectDatabaseTests
{
    [Fact]
    public async Task PendingConcurrencyMigrationInitializesMetadataForExistingActivities()
    {
        using var project = new TestProject();
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        var databasePath = Path.Combine(databaseDirectory, "data.db");
        Directory.CreateDirectory(databaseDirectory);

        await using (var context = new DboxDbContextFactory().Create(databasePath))
        {
            await context.Database.MigrateAsync("20260820191637_InitialCreate");
        }

        var connectionString = new DbConnectionStringBuilder
        {
            ["Data Source"] = databasePath
        }.ConnectionString;
        var legacyOptions = new DbContextOptionsBuilder<LegacyDboxDbContext>()
            .UseSqlite(connectionString)
            .Options;
        await using (var legacyContext = new LegacyDboxDbContext(legacyOptions))
        {
            legacyContext.Activities.Add(new LegacyActivity
            {
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Type = "research",
                Title = "Legacy",
                Description = "Legacy details",
                Status = "completed",
                Source = "manual",
                Area = "backend",
                Result = "Legacy result",
                Impact = "Legacy impact",
                Effort = "low"
            });
            await legacyContext.SaveChangesAsync();
        }

        var list = await TestProject.RunAsync(project.Root, "activity", "list");
        Assert.True(list.ExitCode == 0, list.Error);
        Assert.Empty(list.Error);
        using var listDocument = JsonDocument.Parse(list.Output);
        var activity = listDocument.RootElement.GetProperty("items")[0];
        var add = await TestProject.RunAsync(
            project.Root,
            "activity",
            "add",
            "--json",
            "{\"type\":\"research\",\"title\":\"New\",\"description\":\"Details\",\"status\":\"pending\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Result\",\"impact\":\"Impact\",\"effort\":\"low\"}");
        using var addDocument = JsonDocument.Parse(add.Output);

        Assert.Equal(1, listDocument.RootElement.GetProperty("pagination").GetProperty("total").GetInt32());
        Assert.Equal(1, activity.GetProperty("version").GetInt64());
        Assert.Matches(
            "^\\d{4}-\\d{2}-\\d{2}T\\d{2}:\\d{2}:\\d{2}(?:\\.\\d+)?Z$",
            activity.GetProperty("updated_at").GetString());
        Assert.Equal(0, add.ExitCode);
        Assert.Equal(1, addDocument.RootElement.GetProperty("version").GetInt64());
    }

    [Fact]
    public async Task ParentDatabaseIsDiscoveredFromNestedDirectory()
    {
        using var project = new TestProject();
        var nested = project.CreateChild("src/feature");

        await TestProject.RunAsync(project.Root, "init");
        await TestProject.RunAsync(project.Root, "activity", "add", "--json", "{\"type\":\"research\",\"title\":\"Parent activity\",\"description\":\"Parent details\",\"status\":\"completed\",\"source\":\"manual\",\"area\":\"backend\",\"result\":\"Parent result\",\"impact\":\"Parent impact\",\"effort\":\"low\"}");
        var result = await TestProject.RunAsync(nested, "activity", "list");

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
        Assert.Contains("database_not_found", result.Error);
    }

    [Fact]
    public async Task DatabasePathsWithConnectionStringCharactersRemainUsable()
    {
        using var project = new TestProject();
        var specialDirectory = project.CreateChild("project;Mode=Memory'quoted");

        var init = await TestProject.RunAsync(specialDirectory, "init");
        var list = await TestProject.RunAsync(specialDirectory, "activity", "list");

        Assert.Equal(0, init.ExitCode);
        Assert.Equal(0, list.ExitCode);
        Assert.Contains("\"items\": []", list.Output);
    }

    [Fact]
    public async Task ExistingEmptyDatabaseIsMigratedBeforeACommand()
    {
        using var project = new TestProject();
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        Directory.CreateDirectory(databaseDirectory);
        File.WriteAllBytes(Path.Combine(databaseDirectory, "data.db"), []);

        var list = await TestProject.RunAsync(project.Root, "activity", "list");

        Assert.Equal(0, list.ExitCode);
        Assert.Contains("\"items\": []", list.Output);
    }

    [Fact]
    public async Task InvalidDatabaseReturnsDatabaseErrorWithoutOutput()
    {
        using var project = new TestProject();
        var databaseDirectory = Path.Combine(project.Root, ".dbox");
        Directory.CreateDirectory(databaseDirectory);
        File.WriteAllText(Path.Combine(databaseDirectory, "data.db"), "not a sqlite database");

        var result = await TestProject.RunAsync(project.Root, "activity", "list");
        var init = await TestProject.RunAsync(project.Root, "init");

        Assert.Equal(4, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.Contains("database_error", result.Error);
        Assert.Equal(4, init.ExitCode);
        Assert.Empty(init.Output);
        Assert.Contains("database_error", init.Error);
    }

    private sealed class LegacyDboxDbContext(DbContextOptions<LegacyDboxDbContext> options) : DbContext(options)
    {
        public DbSet<LegacyActivity> Activities => Set<LegacyActivity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var activity = modelBuilder.Entity<LegacyActivity>();
            activity.ToTable("activities");
            activity.HasKey(item => item.Id);
            activity.Property(item => item.Id).HasColumnName("id").ValueGeneratedOnAdd();
            activity.Property(item => item.CreatedAt).HasColumnName("created_at").HasColumnType("TEXT").IsRequired();
            activity.Property(item => item.Type).HasColumnName("type").IsRequired();
            activity.Property(item => item.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
            activity.Property(item => item.Description).HasColumnName("description").IsRequired();
            activity.Property(item => item.Status).HasColumnName("status").IsRequired();
            activity.Property(item => item.Source).HasColumnName("source").IsRequired();
            activity.Property(item => item.Area).HasColumnName("area").IsRequired();
            activity.Property(item => item.Result).HasColumnName("result").IsRequired();
            activity.Property(item => item.Impact).HasColumnName("impact").IsRequired();
            activity.Property(item => item.Effort).HasColumnName("effort").IsRequired();
            activity.Property(item => item.Reference).HasColumnName("reference");
            activity.Property(item => item.Metadata).HasColumnName("metadata").HasColumnType("TEXT");
        }
    }

    private sealed class LegacyActivity
    {
        public long Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public string Effort { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public string? Metadata { get; set; }
    }
}
