using Dbox.Activities;
using Microsoft.EntityFrameworkCore;

namespace Dbox.Database;

public sealed class DboxDbContext(DbContextOptions<DboxDbContext> options) : DbContext(options)
{
    public DbSet<Activity> Activities => Set<Activity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var activity = modelBuilder.Entity<Activity>();
        activity.ToTable(ActivitySchema.TableName);
        activity.HasKey(item => item.Id);
        var id = ActivitySchema.Field("id");
        var createdAt = ActivitySchema.Field("created_at");
        var updatedAt = ActivitySchema.Field("updated_at");
        var version = ActivitySchema.Field("version");
        var type = ActivitySchema.Field("type");
        var title = ActivitySchema.Field("title");
        var description = ActivitySchema.Field("description");
        var status = ActivitySchema.Field("status");
        var source = ActivitySchema.Field("source");
        var area = ActivitySchema.Field("area");
        var result = ActivitySchema.Field("result");
        var impact = ActivitySchema.Field("impact");
        var effort = ActivitySchema.Field("effort");
        var reference = ActivitySchema.Field("reference");
        var metadata = ActivitySchema.Field("metadata");

        activity.Property(item => item.Id)
            .HasColumnName(id.Name)
            .ValueGeneratedOnAdd();

        activity.Property(item => item.CreatedAt)
            .HasColumnName(createdAt.Name)
            .HasColumnType("TEXT")
            .IsRequired(createdAt.Required);

        activity.Property(item => item.UpdatedAt)
            .HasColumnName(updatedAt.Name)
            .HasColumnType("TEXT")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd()
            .IsRequired(updatedAt.Required);

        activity.Property(item => item.Version)
            .HasColumnName(version.Name)
            .HasDefaultValue(ActivitySchema.InitialVersion)
            .ValueGeneratedOnAdd()
            .IsConcurrencyToken()
            .IsRequired(version.Required);

        activity.Property(item => item.Type)
            .HasColumnName(type.Name)
            .IsRequired(type.Required);

        activity.Property(item => item.Title)
            .HasColumnName(title.Name)
            .HasMaxLength(title.MaxLength!.Value)
            .IsRequired(title.Required);

        activity.Property(item => item.Description)
            .HasColumnName(description.Name)
            .IsRequired(description.Required);

        activity.Property(item => item.Status)
            .HasColumnName(status.Name)
            .IsRequired(status.Required);

        activity.Property(item => item.Source)
            .HasColumnName(source.Name)
            .IsRequired(source.Required);

        activity.Property(item => item.Area)
            .HasColumnName(area.Name)
            .IsRequired(area.Required);

        activity.Property(item => item.Result)
            .HasColumnName(result.Name)
            .IsRequired(result.Required);

        activity.Property(item => item.Impact)
            .HasColumnName(impact.Name)
            .IsRequired(impact.Required);

        activity.Property(item => item.Effort)
            .HasColumnName(effort.Name)
            .IsRequired(effort.Required);

        activity.Property(item => item.Reference)
            .HasColumnName(reference.Name)
            .IsRequired(reference.Required);

        activity.Property(item => item.Metadata)
            .HasColumnName(metadata.Name)
            .HasColumnType("TEXT")
            .IsRequired(metadata.Required);
    }
}
