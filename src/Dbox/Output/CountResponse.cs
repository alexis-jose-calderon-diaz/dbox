using System.Text.Json.Serialization;

namespace Dbox.Output;

public sealed record CountResponse([property: JsonPropertyName("count")] int Count);
