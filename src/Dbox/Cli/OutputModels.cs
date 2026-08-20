using System.Text.Json.Serialization;

namespace Dbox.Cli;

public sealed record InitResponse(
    [property: JsonPropertyName("database")] string Database,
    [property: JsonPropertyName("status")] string Status);

public sealed record DeleteResponse(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("deleted")] bool Deleted);
