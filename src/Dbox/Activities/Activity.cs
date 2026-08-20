namespace Dbox.Activities;

public sealed class Activity
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = ActivitySchema.DefaultStatus;
}
