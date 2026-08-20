using System.Text.Json.Serialization;

namespace Dbox.Output;

public sealed record DeleteResponse(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("deleted")] bool Deleted);
