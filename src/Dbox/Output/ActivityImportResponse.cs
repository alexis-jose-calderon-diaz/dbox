using System.Text.Json.Serialization;

namespace Dbox.Output;

public sealed record ActivityImportResponse(
    [property: JsonPropertyName("imported")] int Imported,
    [property: JsonPropertyName("format")] string Format);
