using System.Text.Json.Serialization;

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

public sealed record ActivityView(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("status")] string Status)
{
    public static ActivityView FromEntity(Activity activity)
    {
        var createdAt = DateTime.SpecifyKind(activity.CreatedAt, DateTimeKind.Utc);
        return new ActivityView(
            activity.Id,
            createdAt.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", System.Globalization.CultureInfo.InvariantCulture),
            activity.Type,
            activity.Title,
            activity.Description,
            activity.Status);
    }
}

public sealed record ActivityCreateInput(
    string? Type,
    string? Title,
    string? Description,
    string? Status,
    bool TypeProvided,
    bool TitleProvided,
    bool DescriptionProvided,
    bool StatusProvided);

public sealed class ActivityUpdateInput
{
    public string? Type { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? Status { get; init; }

    public bool TypeProvided { get; init; }

    public bool TitleProvided { get; init; }

    public bool DescriptionProvided { get; init; }

    public bool StatusProvided { get; init; }

    public bool HasChanges => TypeProvided || TitleProvided || DescriptionProvided || StatusProvided;
}
