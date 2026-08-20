using System.Text.Json.Serialization;

namespace Dbox.Activities;

public sealed class ActivitySchemaEntity
{
    [JsonPropertyName("fields")]
    public Dictionary<string, ActivitySchemaField> Fields { get; init; } = new(StringComparer.Ordinal);
}
