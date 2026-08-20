namespace Dbox.Activities;

public sealed record ActivityFieldDefinition(
    string Name,
    string Type,
    bool Required,
    bool Generated,
    bool Mutable,
    IReadOnlyList<string>? EnumValues = null,
    int? MaxLength = null,
    bool NonBlank = false,
    bool Nullable = false,
    string? Description = null);
