using System.Text.Json.Serialization;

namespace Dbox.Activities;

public sealed class ActivitySchemaField
{
    [JsonPropertyName("name")]
    [JsonPropertyOrder(0)]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("type")]
    [JsonPropertyOrder(1)]
    public string Type { get; init; } = string.Empty;

    [JsonPropertyName("required")]
    [JsonPropertyOrder(2)]
    public bool Required { get; init; }

    [JsonPropertyName("generated")]
    [JsonPropertyOrder(3)]
    public bool Generated { get; init; }

    [JsonPropertyName("mutable")]
    [JsonPropertyOrder(4)]
    public bool Mutable { get; init; }

    [JsonPropertyName("enum")]
    [JsonPropertyOrder(5)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Enum { get; init; }

    [JsonPropertyName("maxLength")]
    [JsonPropertyOrder(6)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxLength { get; init; }

    [JsonPropertyName("nullable")]
    [JsonPropertyOrder(7)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Nullable { get; init; }

    [JsonPropertyName("default")]
    [JsonPropertyOrder(8)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Default { get; init; }

    [JsonPropertyName("description")]
    [JsonPropertyOrder(9)]
    public string Description { get; init; } = string.Empty;

}
