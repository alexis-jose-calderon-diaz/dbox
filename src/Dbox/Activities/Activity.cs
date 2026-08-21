namespace Dbox.Activities;

public sealed class Activity
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public long Version { get; set; }

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
