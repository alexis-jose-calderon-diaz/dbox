using System.Text.Json.Serialization;

namespace Dbox.Activities;

public sealed class ActivitySchemaDocument
{
    [JsonPropertyName("entities")]
    public Dictionary<string, ActivitySchemaEntity> Entities { get; init; } = new(StringComparer.Ordinal);
}
