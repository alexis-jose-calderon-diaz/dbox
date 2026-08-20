using System.Text.Json.Serialization;

namespace Dbox.Output;

public sealed record InitResponse(
    [property: JsonPropertyName("database")] string Database,
    [property: JsonPropertyName("status")] string Status);
