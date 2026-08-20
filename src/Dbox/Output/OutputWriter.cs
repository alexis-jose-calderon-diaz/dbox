using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dbox.Activities;
using Dbox.Cli;

namespace Dbox.Output;

public sealed class OutputWriter(TextWriter output, TextWriter error)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public void WriteSuccess(object? value, OutputFormat format)
    {
        if (format == OutputFormat.Json)
        {
            output.WriteLine(JsonSerializer.Serialize(value, JsonOptions));
            return;
        }

        switch (value)
        {
            case InitResponse init:
                WriteInitText(init);
                break;
            case ActivitySchemaDocument schema:
                WriteSchemaText(schema);
                break;
            case ActivityView activity:
                WriteActivityText(activity);
                break;
            case IReadOnlyList<ActivityView> activities:
                WriteActivitiesText(activities);
                break;
            case DeleteResponse deletion:
                output.WriteLine($"Activity {deletion.Id} deleted.");
                break;
            default:
                throw new InvalidOperationException($"Unsupported output type: {value?.GetType().Name ?? "null"}.");
        }
    }

    public void WriteError(CliError cliError, OutputFormat format)
    {
        if (format == OutputFormat.Json)
        {
            var envelope = new ErrorEnvelope
            {
                Error = new ErrorBody
                {
                    Code = cliError.Code,
                    Message = cliError.Message,
                    Details = cliError.Details
                }
            };
            error.WriteLine(JsonSerializer.Serialize(envelope, JsonOptions));
            return;
        }

        if (cliError.Code == "validation_error")
        {
            error.WriteLine("Validation error:");
            foreach (var detail in cliError.Details ?? [])
            {
                error.WriteLine($"{detail.Field}: {detail.Message}");
            }

            return;
        }

        error.WriteLine(cliError.Message);
    }

    private void WriteInitText(InitResponse init)
    {
        var message = init.Status switch
        {
            "initialized" => "Database initialized: .dbox/data.db",
            "already_initialized" => "Database already initialized: .dbox/data.db",
            "migrated" => "Database migrated: .dbox/data.db",
            _ => $"Database status: {init.Status}"
        };
        output.WriteLine(message);
    }

    private void WriteSchemaText(ActivitySchemaDocument _)
    {
        output.WriteLine("Entity: activity");
        output.WriteLine("Fields:");
        foreach (var field in ActivitySchema.Fields)
        {
            var rules = new List<string> { field.Type };
            if (field.Required)
            {
                rules.Add("required");
            }

            if (field.Generated)
            {
                rules.Add("generated");
            }

            if (!field.Mutable)
            {
                rules.Add("read-only");
            }

            if (field.EnumValues is not null)
            {
                rules.Add($"one of {string.Join(", ", field.EnumValues)}");
            }

            if (field.MaxLength is not null)
            {
                rules.Add($"max {field.MaxLength} chars");
            }

            if (field.DefaultValue is not null)
            {
                rules.Add($"default {field.DefaultValue}");
            }

            if (field.NonBlank)
            {
                rules.Add("non-blank");
            }

            if (field.Nullable)
            {
                rules.Add("optional, nullable");
            }

            output.WriteLine($"  {field.Name}: {string.Join(", ", rules)}");
        }
    }

    private void WriteActivityText(ActivityView activity)
    {
        output.WriteLine($"ID: {activity.Id}");
        output.WriteLine($"CREATED_AT: {EscapeText(activity.CreatedAt)}");
        output.WriteLine($"TYPE: {EscapeText(activity.Type)}");
        output.WriteLine($"TITLE: {EscapeText(activity.Title)}");
        output.WriteLine($"DESCRIPTION: {EscapeText(activity.Description ?? "")}");
        output.WriteLine($"STATUS: {EscapeText(activity.Status)}");
    }

    private void WriteActivitiesText(IReadOnlyList<ActivityView> activities)
    {
        output.WriteLine("ID\tCREATED_AT\tTYPE\tSTATUS\tTITLE");
        foreach (var activity in activities)
        {
            output.WriteLine($"{activity.Id}\t{EscapeText(activity.CreatedAt)}\t{EscapeText(activity.Type)}\t{EscapeText(activity.Status)}\t{EscapeText(activity.Title)}");
        }
    }

    private static string EscapeText(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            switch (character)
            {
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    if (char.IsControl(character))
                    {
                        builder.Append($"\\u{(int)character:X4}");
                    }
                    else
                    {
                        builder.Append(character);
                    }

                    break;
            }
        }

        return builder.ToString();
    }

    private sealed class ErrorEnvelope
    {
        [JsonPropertyName("error")]
        public ErrorBody Error { get; init; } = new();
    }

    private sealed class ErrorBody
    {
        [JsonPropertyName("code")]
        public string Code { get; init; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; init; } = string.Empty;

        [JsonPropertyName("details")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyList<ErrorDetail>? Details { get; init; }
    }
}
