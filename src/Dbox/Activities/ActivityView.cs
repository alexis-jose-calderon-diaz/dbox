using System.Text.Json.Serialization;

namespace Dbox.Activities;

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
