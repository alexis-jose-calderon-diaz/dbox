namespace Dbox.Activities;

public static class ActivitySchema
{
    public const string EntityName = "activity";
    public const string TableName = "activities";
    public const string DefaultStatus = "completed";
    public const int TitleMaxLength = 200;

    public static IReadOnlyList<string> Types { get; } =
        ["research", "implementation", "bugfix", "maintenance"];

    public static IReadOnlyList<string> Statuses { get; } =
        ["pending", "in_progress", "completed"];

    public static IReadOnlyList<ActivityFieldDefinition> Fields { get; } =
    [
        new("id", "integer", Required: true, Generated: true, Mutable: false),
        new("created_at", "datetime", Required: true, Generated: true, Mutable: false),
        new("type", "string", Required: true, Generated: false, Mutable: true, EnumValues: Types),
        new("title", "string", Required: true, Generated: false, Mutable: true, MaxLength: TitleMaxLength, NonBlank: true),
        new("description", "string", Required: false, Generated: false, Mutable: true, Nullable: true),
        new("status", "string", Required: true, Generated: false, Mutable: true, EnumValues: Statuses, DefaultValue: DefaultStatus)
    ];

    public static ActivityFieldDefinition Field(string name) =>
        Fields.Single(field => string.Equals(field.Name, name, StringComparison.Ordinal));

    public static bool IsType(string? value) => value is not null && Types.Contains(value, StringComparer.Ordinal);

    public static bool IsStatus(string? value) => value is not null && Statuses.Contains(value, StringComparer.Ordinal);

    public static ActivitySchemaDocument CreateDocument()
    {
        var fields = new Dictionary<string, ActivitySchemaField>(StringComparer.Ordinal);

        foreach (var field in Fields)
        {
            fields[field.Name] = new ActivitySchemaField
            {
                Type = field.Type,
                Required = field.Generated ? null : field.Required,
                Generated = field.Generated ? true : null,
                Mutable = field.Generated ? false : null,
                Enum = field.EnumValues,
                MaxLength = field.MaxLength,
                Default = field.DefaultValue
            };
        }

        return new ActivitySchemaDocument
        {
            Entities = new Dictionary<string, ActivitySchemaEntity>(StringComparer.Ordinal)
            {
                [EntityName] = new ActivitySchemaEntity { Fields = fields }
            }
        };
    }
}
