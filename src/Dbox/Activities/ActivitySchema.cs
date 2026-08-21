namespace Dbox.Activities;

public static class ActivitySchema
{
    public const string EntityName = "activity";
    public const string TableName = "activities";
    public const int TitleMaxLength = 200;
    public const long InitialVersion = 1;

    public static IReadOnlyList<string> Statuses { get; } =
        ["pending", "in_progress", "completed"];

    public static IReadOnlyList<string> Efforts { get; } =
        ["low", "medium", "high", "very-high"];

    public static IReadOnlyList<ActivityFieldDefinition> Fields { get; } =
    [
        new("id", "integer", Required: true, Generated: true, Mutable: false, Description: "Identificador entero generado automaticamente."),
        new("created_at", "datetime", Required: true, Generated: true, Mutable: false, Description: "Fecha y hora UTC generada al crear la actividad."),
        new("updated_at", "datetime", Required: true, Generated: true, Mutable: false, Description: "Fecha y hora UTC de la ultima modificacion exitosa."),
        new("version", "integer", Required: true, Generated: true, Mutable: false, Description: "Version positiva generada para controlar concurrencia.", DefaultValue: InitialVersion),
        new("type", "string", Required: true, Generated: false, Mutable: true, NonBlank: true, Description: "Clasificacion extensible de la actividad."),
        new("title", "string", Required: true, Generated: false, Mutable: true, MaxLength: TitleMaxLength, NonBlank: true, Description: "Titulo breve de la actividad."),
        new("description", "string", Required: true, Generated: false, Mutable: true, NonBlank: true, Description: "Descripcion de lo realizado."),
        new("status", "string", Required: true, Generated: false, Mutable: true, EnumValues: Statuses, NonBlank: true, Description: "Estado actual de la actividad."),
        new("source", "string", Required: true, Generated: false, Mutable: true, NonBlank: true, Description: "Origen o motivacion de la actividad."),
        new("area", "string", Required: true, Generated: false, Mutable: true, NonBlank: true, Description: "Area funcional o tecnica afectada."),
        new("result", "string", Required: true, Generated: false, Mutable: true, NonBlank: true, Description: "Resultado concreto obtenido."),
        new("impact", "string", Required: true, Generated: false, Mutable: true, NonBlank: true, Description: "Mejora, beneficio o consecuencia producida."),
        new("effort", "string", Required: true, Generated: false, Mutable: true, EnumValues: Efforts, NonBlank: true, Description: "Estimacion cualitativa del esfuerzo."),
        new("reference", "string", Required: false, Generated: false, Mutable: true, Nullable: true, Description: "Referencia textual opcional relacionada."),
        new("metadata", "json", Required: false, Generated: false, Mutable: true, Nullable: true, Description: "Objeto JSON opcional con informacion adicional extensible.")
    ];

    public static ActivityFieldDefinition Field(string name) =>
        Fields.Single(field => string.Equals(field.Name, name, StringComparison.Ordinal));

    public static bool IsStatus(string? value) => value is not null && Statuses.Contains(value, StringComparer.Ordinal);

    public static bool IsEffort(string? value) => value is not null && Efforts.Contains(value, StringComparer.Ordinal);

    public static ActivitySchemaDocument CreateDocument()
    {
        var fields = new Dictionary<string, ActivitySchemaField>(StringComparer.Ordinal);

        foreach (var field in Fields)
        {
            fields[field.Name] = new ActivitySchemaField
            {
                Name = field.Name,
                Type = field.Type,
                Required = field.Generated ? false : field.Required,
                Generated = field.Generated,
                Mutable = field.Mutable,
                Enum = field.EnumValues,
                MaxLength = field.MaxLength,
                Nullable = field.Nullable ? true : null,
                Default = field.DefaultValue,
                Description = field.Description ?? string.Empty
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
