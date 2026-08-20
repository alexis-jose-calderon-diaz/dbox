using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dbox.Activities;

public sealed record ActivityView(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("area")] string Area,
    [property: JsonPropertyName("result")] string Result,
    [property: JsonPropertyName("impact")] string Impact,
    [property: JsonPropertyName("effort")] string Effort,
    [property: JsonPropertyName("reference")] string? Reference,
    [property: JsonPropertyName("metadata")] JsonElement? Metadata)
{
    public static ActivityView FromEntity(Activity activity)
    {
        var createdAt = DateTime.SpecifyKind(activity.CreatedAt, DateTimeKind.Utc);
        return new ActivityView(
            activity.Id,
            createdAt.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
            activity.Type,
            activity.Title,
            activity.Description,
            activity.Status,
            activity.Source,
            activity.Area,
            activity.Result,
            activity.Impact,
            activity.Effort,
            activity.Reference,
            ParseMetadata(activity.Metadata));
    }

    private static JsonElement? ParseMetadata(string? metadata)
    {
        if (metadata is null)
        {
            return null;
        }

        using var document = JsonDocument.Parse(metadata);
        return document.RootElement.Clone();
    }
}
