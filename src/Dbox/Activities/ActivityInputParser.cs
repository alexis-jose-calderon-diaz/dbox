using System.Text.Json;

namespace Dbox.Activities;

public static class ActivityInputParser
{
    public static InputResult<ActivityCreateInput> ParseCreate(
        bool jsonProvided,
        string? json,
        bool typeProvided,
        string? type,
        bool titleProvided,
        string? title,
        bool descriptionProvided,
        string? description,
        bool statusProvided,
        string? status)
    {
        if (jsonProvided && (typeProvided || titleProvided || descriptionProvided || statusProvided))
        {
            return InputResult<ActivityCreateInput>.Failure(
                new ValidationIssue("input", "Use --json or field options, but not both."));
        }

        if (jsonProvided)
        {
            return ParseCreateJson(json);
        }

        return InputResult<ActivityCreateInput>.Success(new ActivityCreateInput(
            type,
            title,
            description,
            status,
            typeProvided,
            titleProvided,
            descriptionProvided,
            statusProvided));
    }

    public static InputResult<ActivityUpdateInput> ParseUpdate(
        bool jsonProvided,
        string? json,
        bool typeProvided,
        string? type,
        bool titleProvided,
        string? title,
        bool descriptionProvided,
        string? description,
        bool statusProvided,
        string? status)
    {
        if (jsonProvided && (typeProvided || titleProvided || descriptionProvided || statusProvided))
        {
            return InputResult<ActivityUpdateInput>.Failure(
                new ValidationIssue("input", "Use --json or field options, but not both."));
        }

        if (jsonProvided)
        {
            return ParseUpdateJson(json);
        }

        return InputResult<ActivityUpdateInput>.Success(new ActivityUpdateInput
        {
            Type = type,
            Title = title,
            Description = description,
            Status = status,
            TypeProvided = typeProvided,
            TitleProvided = titleProvided,
            DescriptionProvided = descriptionProvided,
            StatusProvided = statusProvided
        });
    }

    private static InputResult<ActivityCreateInput> ParseCreateJson(string? json)
    {
        var values = new JsonValues();
        var issues = ReadJsonObject(json, values, allowDescriptionNull: true);
        if (issues.Count > 0)
        {
            return new InputResult<ActivityCreateInput>(default, issues);
        }

        return InputResult<ActivityCreateInput>.Success(new ActivityCreateInput(
            values.Type,
            values.Title,
            values.Description,
            values.Status,
            values.TypeProvided,
            values.TitleProvided,
            values.DescriptionProvided,
            values.StatusProvided));
    }

    private static InputResult<ActivityUpdateInput> ParseUpdateJson(string? json)
    {
        var values = new JsonValues();
        var issues = ReadJsonObject(json, values, allowDescriptionNull: true);
        if (issues.Count > 0)
        {
            return new InputResult<ActivityUpdateInput>(default, issues);
        }

        return InputResult<ActivityUpdateInput>.Success(new ActivityUpdateInput
        {
            Type = values.Type,
            Title = values.Title,
            Description = values.Description,
            Status = values.Status,
            TypeProvided = values.TypeProvided,
            TitleProvided = values.TitleProvided,
            DescriptionProvided = values.DescriptionProvided,
            StatusProvided = values.StatusProvided
        });
    }

    private static List<ValidationIssue> ReadJsonObject(string? json, JsonValues values, bool allowDescriptionNull)
    {
        var issues = new List<ValidationIssue>();
        if (string.IsNullOrWhiteSpace(json))
        {
            issues.Add(new ValidationIssue("json", "A JSON object is required."));
            return issues;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                issues.Add(new ValidationIssue("json", "The JSON input must be an object."));
                return issues;
            }

            foreach (var property in document.RootElement.EnumerateObject())
            {
                switch (property.Name)
                {
                    case "type":
                        values.TypeProvided = true;
                        values.Type = ReadString(property, issues);
                        break;
                    case "title":
                        values.TitleProvided = true;
                        values.Title = ReadString(property, issues);
                        break;
                    case "description":
                        values.DescriptionProvided = true;
                        values.Description = ReadNullableString(property, issues, allowDescriptionNull);
                        break;
                    case "status":
                        values.StatusProvided = true;
                        values.Status = ReadString(property, issues);
                        break;
                    case "id":
                    case "created_at":
                        issues.Add(new ValidationIssue(property.Name, "Field is generated and read-only."));
                        break;
                    default:
                        issues.Add(new ValidationIssue(property.Name, "Unknown property."));
                        break;
                }
            }
        }
        catch (JsonException)
        {
            issues.Add(new ValidationIssue("json", "The JSON input is invalid."));
        }

        return issues;
    }

    private static string? ReadString(JsonProperty property, ICollection<ValidationIssue> issues)
    {
        if (property.Value.ValueKind != JsonValueKind.String)
        {
            issues.Add(new ValidationIssue(property.Name, "Value must be a string."));
            return null;
        }

        return property.Value.GetString();
    }

    private static string? ReadNullableString(
        JsonProperty property,
        ICollection<ValidationIssue> issues,
        bool allowNull)
    {
        if (property.Value.ValueKind == JsonValueKind.Null && allowNull)
        {
            return null;
        }

        return ReadString(property, issues);
    }

    private sealed class JsonValues
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public bool TypeProvided { get; set; }
        public bool TitleProvided { get; set; }
        public bool DescriptionProvided { get; set; }
        public bool StatusProvided { get; set; }
    }
}
