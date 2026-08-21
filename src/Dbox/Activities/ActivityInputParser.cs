using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Dbox.Activities;

public static class ActivityInputParser
{
    public const string ConflictingJsonSourcesMessage =
        "Specify either '--json' or '--json-file', not both.";

    public const string UnreadableJsonInputMessage = "Unable to read JSON input.";

    public const string InvalidJsonInputMessage = "JSON input must be a valid JSON object.";

    public static async Task<InputResult<string?>> ReadJsonAsync(
        string? inlineJson,
        string? filePath,
        TextReader input,
        bool required,
        CancellationToken cancellationToken,
        string? baseDirectory = null)
    {
        if (inlineJson is not null && filePath is not null)
        {
            return InvalidSource(ConflictingJsonSourcesMessage);
        }

        if (inlineJson is not null)
        {
            return InputResult<string?>.Success(inlineJson);
        }

        if (filePath is null)
        {
            return required
                ? new InputResult<string?>(
                    default,
                    [new ValidationIssue("json", "A JSON input is required.")])
                : InputResult<string?>.Success(null);
        }

        try
        {
            var resolvedPath = filePath == "-" || Path.IsPathRooted(filePath) || baseDirectory is null
                ? filePath
                : Path.Combine(baseDirectory, filePath);
            return InputResult<string?>.Success(filePath == "-"
                ? await input.ReadToEndAsync(cancellationToken)
                : await File.ReadAllTextAsync(
                    resolvedPath,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
                    cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return InvalidSource(UnreadableJsonInputMessage);
        }
    }

    public static InputResult<ActivityCreateInput> ParseCreate(string? json)
    {
        var values = new JsonValues();
        var issues = ReadJsonObject(json, values, allowVersion: false, out var errorMessage);
        if (issues.Count > 0)
        {
            return new InputResult<ActivityCreateInput>(default, issues, errorMessage);
        }

        return InputResult<ActivityCreateInput>.Success(new ActivityCreateInput(
            values.Type,
            values.Title,
            values.Description,
            values.Status,
            values.Source,
            values.Area,
            values.Result,
            values.Impact,
            values.Effort,
            values.Reference,
            values.Metadata,
            values.TypeProvided,
            values.TitleProvided,
            values.DescriptionProvided,
            values.StatusProvided,
            values.SourceProvided,
            values.AreaProvided,
            values.ResultProvided,
            values.ImpactProvided,
            values.EffortProvided,
            values.ReferenceProvided,
            values.MetadataProvided));
    }

    public static InputResult<ActivityUpdateInput> ParseUpdate(string? json)
    {
        var values = new JsonValues();
        var issues = ReadJsonObject(json, values, allowVersion: true, out var errorMessage);
        if (issues.Count > 0)
        {
            return new InputResult<ActivityUpdateInput>(default, issues, errorMessage);
        }

        return InputResult<ActivityUpdateInput>.Success(new ActivityUpdateInput
        {
            Version = values.Version,
            Type = values.Type,
            Title = values.Title,
            Description = values.Description,
            Status = values.Status,
            Source = values.Source,
            Area = values.Area,
            Result = values.Result,
            Impact = values.Impact,
            Effort = values.Effort,
            Reference = values.Reference,
            Metadata = values.Metadata,
            TypeProvided = values.TypeProvided,
            TitleProvided = values.TitleProvided,
            DescriptionProvided = values.DescriptionProvided,
            StatusProvided = values.StatusProvided,
            SourceProvided = values.SourceProvided,
            AreaProvided = values.AreaProvided,
            ResultProvided = values.ResultProvided,
            ImpactProvided = values.ImpactProvided,
            EffortProvided = values.EffortProvided,
            ReferenceProvided = values.ReferenceProvided,
            MetadataProvided = values.MetadataProvided,
            VersionProvided = values.VersionProvided
        });
    }

    public static InputResult<ActivityFilter> ParseFilter(string? json)
    {
        if (json is null)
        {
            return InputResult<ActivityFilter>.Success(EmptyFilter());
        }

        var issues = new List<ValidationIssue>();
        string? type = null;
        string? status = null;
        string? area = null;
        string? source = null;
        string? effort = null;
        DateTime? createdFrom = null;
        DateTime? createdTo = null;
        string? title = null;
        string? description = null;
        string? errorMessage = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return InvalidJson<ActivityFilter>();
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return InvalidJson<ActivityFilter>();
            }

            foreach (var property in document.RootElement.EnumerateObject())
            {
                switch (property.Name)
                {
                    case "type":
                        type = ReadString(property, issues);
                        break;
                    case "status":
                        status = ReadString(property, issues);
                        break;
                    case "area":
                        area = ReadString(property, issues);
                        break;
                    case "source":
                        source = ReadString(property, issues);
                        break;
                    case "effort":
                        effort = ReadString(property, issues);
                        break;
                    case "created_from":
                        createdFrom = ReadUtcDateTime(property, issues);
                        break;
                    case "created_to":
                        createdTo = ReadUtcDateTime(property, issues);
                        break;
                    case "title":
                        title = ReadString(property, issues);
                        break;
                    case "description":
                        description = ReadString(property, issues);
                        break;
                    default:
                        issues.Add(new ValidationIssue(property.Name, "Unknown property."));
                        break;
                }
            }
        }
        catch (JsonException)
        {
            errorMessage = InvalidJsonInputMessage;
            issues.Add(new ValidationIssue("json", InvalidJsonInputMessage));
        }

        var filter = new ActivityFilter(
            type,
            status,
            area,
            source,
            effort,
            createdFrom,
            createdTo,
            title,
            description);
        return issues.Count > 0
            ? new InputResult<ActivityFilter>(filter, issues, errorMessage)
            : InputResult<ActivityFilter>.Success(filter);
    }

    private static List<ValidationIssue> ReadJsonObject(
        string? json,
        JsonValues values,
        bool allowVersion,
        out string? errorMessage)
    {
        var issues = new List<ValidationIssue>();
        errorMessage = null;
        if (string.IsNullOrWhiteSpace(json))
        {
            errorMessage = InvalidJsonInputMessage;
            issues.Add(new ValidationIssue("json", InvalidJsonInputMessage));
            return issues;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                errorMessage = InvalidJsonInputMessage;
                issues.Add(new ValidationIssue("json", InvalidJsonInputMessage));
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
                        values.Description = ReadString(property, issues);
                        break;
                    case "status":
                        values.StatusProvided = true;
                        values.Status = ReadString(property, issues);
                        break;
                    case "source":
                        values.SourceProvided = true;
                        values.Source = ReadString(property, issues);
                        break;
                    case "area":
                        values.AreaProvided = true;
                        values.Area = ReadString(property, issues);
                        break;
                    case "result":
                        values.ResultProvided = true;
                        values.Result = ReadString(property, issues);
                        break;
                    case "impact":
                        values.ImpactProvided = true;
                        values.Impact = ReadString(property, issues);
                        break;
                    case "effort":
                        values.EffortProvided = true;
                        values.Effort = ReadString(property, issues);
                        break;
                    case "reference":
                        values.ReferenceProvided = true;
                        values.Reference = ReadNullableString(property, issues);
                        break;
                    case "metadata":
                        values.MetadataProvided = true;
                        values.Metadata = ReadMetadata(property, issues);
                        break;
                    case "id":
                    case "created_at":
                    case "updated_at":
                        issues.Add(new ValidationIssue(property.Name, "Field is generated and read-only."));
                        break;
                    case "version":
                        if (!allowVersion)
                        {
                            issues.Add(new ValidationIssue(property.Name, "Field is generated and read-only."));
                            break;
                        }

                        values.VersionProvided = true;
                        values.Version = ReadInteger(property, issues);
                        break;
                    default:
                        issues.Add(new ValidationIssue(property.Name, "Unknown property."));
                        break;
                }
            }
        }
        catch (JsonException)
        {
            errorMessage = InvalidJsonInputMessage;
            issues.Add(new ValidationIssue("json", InvalidJsonInputMessage));
        }

        return issues;
    }

    private static DateTime? ReadUtcDateTime(JsonProperty property, ICollection<ValidationIssue> issues)
    {
        if (property.Value.ValueKind != JsonValueKind.String ||
            !TryParseUtcDateTime(property.Value.GetString(), out var value))
        {
            issues.Add(new ValidationIssue(
                property.Name,
                "Value must be a UTC ISO 8601 datetime with a Z offset."));
            return null;
        }

        return value;
    }

    private static bool TryParseUtcDateTime(string? value, out DateTime result)
    {
        var formats = new[]
        {
            "yyyy-MM-dd'T'HH:mm:ss'Z'",
            "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'"
        };

        return DateTime.TryParseExact(
            value,
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out result);
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

    private static string? ReadNullableString(JsonProperty property, ICollection<ValidationIssue> issues)
    {
        if (property.Value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return ReadString(property, issues);
    }

    private static string? ReadMetadata(JsonProperty property, ICollection<ValidationIssue> issues)
    {
        if (property.Value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (property.Value.ValueKind != JsonValueKind.Object)
        {
            issues.Add(new ValidationIssue(property.Name, "Value must be a JSON object or null."));
            return null;
        }

        return property.Value.GetRawText();
    }

    private static long? ReadInteger(JsonProperty property, ICollection<ValidationIssue> issues)
    {
        if (property.Value.ValueKind != JsonValueKind.Number ||
            !property.Value.TryGetInt64(out var value))
        {
            issues.Add(new ValidationIssue(property.Name, "Value must be an integer."));
            return null;
        }

        return value;
    }

    private static ActivityFilter EmptyFilter() => new(null, null, null, null, null, null, null, null, null);

    private static InputResult<T> InvalidJson<T>() => new(
        default,
        [new ValidationIssue("json", InvalidJsonInputMessage)],
        InvalidJsonInputMessage);

    private static InputResult<string?> InvalidSource(string message) => new(
        default,
        [new ValidationIssue("json", message)],
        message);

    private sealed class JsonValues
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Source { get; set; }
        public string? Area { get; set; }
        public string? Result { get; set; }
        public string? Impact { get; set; }
        public string? Effort { get; set; }
        public string? Reference { get; set; }
        public string? Metadata { get; set; }
        public long? Version { get; set; }
        public bool TypeProvided { get; set; }
        public bool TitleProvided { get; set; }
        public bool DescriptionProvided { get; set; }
        public bool StatusProvided { get; set; }
        public bool SourceProvided { get; set; }
        public bool AreaProvided { get; set; }
        public bool ResultProvided { get; set; }
        public bool ImpactProvided { get; set; }
        public bool EffortProvided { get; set; }
        public bool ReferenceProvided { get; set; }
        public bool MetadataProvided { get; set; }
        public bool VersionProvided { get; set; }
    }
}
