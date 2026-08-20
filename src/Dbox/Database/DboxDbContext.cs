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
        var type = ActivitySchema.Field("type");
        var title = ActivitySchema.Field("title");
        var description = ActivitySchema.Field("description");
        var status = ActivitySchema.Field("status");

        activity.Property(item => item.Id)
            .HasColumnName(id.Name)
            .ValueGeneratedOnAdd();

        activity.Property(item => item.CreatedAt)
            .HasColumnName(createdAt.Name)
            .HasColumnType("TEXT")
            .IsRequired(createdAt.Required);

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
    }
}
