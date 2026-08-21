using System.Text.Json;

namespace Dbox.Activities;

public static class PortableActivityParser
{
    private static readonly string[] FieldNames =
    [
        "id",
        "created_at",
        "updated_at",
        "version",
        "type",
        "title",
        "description",
        "status",
        "source",
        "area",
        "result",
        "impact",
        "effort",
        "reference",
        "metadata"
    ];

    private static readonly HashSet<string> KnownFields =
        new(FieldNames, StringComparer.Ordinal);

    public const string InvalidJsonMessage = "Portable input must be valid JSON.";

    public static InputResult<IReadOnlyList<PortableActivityRecord>> Parse(
        string? input,
        string format)
    {
        return format switch
        {
            "json" => ParseJson(input),
            "jsonl" => ParseJsonLines(input),
            _ => Invalid(
                new ValidationIssue("format", "Value must be one of: json, jsonl."))
        };
    }

    private static InputResult<IReadOnlyList<PortableActivityRecord>> ParseJson(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Invalid(new ValidationIssue("json", InvalidJsonMessage));
        }

        try
        {
            using var document = JsonDocument.Parse(input);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return Invalid(new ValidationIssue("json", "Portable JSON input must be an array."));
            }

            var records = new List<PortableActivityRecord>();
            var issues = new List<ValidationIssue>();
            var index = 0;
            foreach (var element in document.RootElement.EnumerateArray())
            {
                var recordIssues = new List<ValidationIssue>();
                var record = ParseRecord(element, recordIssues);
                if (record is not null)
                {
                    records.Add(record);
                }

                issues.AddRange(recordIssues.Select(issue => Prefix(index, issue)));
                index++;
            }

            return issues.Count == 0
                ? InputResult<IReadOnlyList<PortableActivityRecord>>.Success(records)
                : new InputResult<IReadOnlyList<PortableActivityRecord>>(records, issues);
        }
        catch (JsonException)
        {
            return Invalid(new ValidationIssue("json", InvalidJsonMessage));
        }
    }

    private static InputResult<IReadOnlyList<PortableActivityRecord>> ParseJsonLines(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return InputResult<IReadOnlyList<PortableActivityRecord>>.Success([]);
        }

        var lines = input.Split('\n');
        if (input.EndsWith('\n'))
        {
            lines = lines[..^1];
        }

        var records = new List<PortableActivityRecord>();
        var issues = new List<ValidationIssue>();
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index].EndsWith('\r')
                ? lines[index][..^1]
                : lines[index];
            if (string.IsNullOrWhiteSpace(line))
            {
                issues.Add(new ValidationIssue(
                    $"line[{index + 1}]",
                    "Blank JSONL lines are not allowed."));
                continue;
            }

            try
            {
                using var document = JsonDocument.Parse(line);
                var recordIssues = new List<ValidationIssue>();
                var record = ParseRecord(document.RootElement, recordIssues);
                if (record is not null)
                {
                    records.Add(record);
                }

                issues.AddRange(recordIssues.Select(issue => Prefix(index, issue)));
            }
            catch (JsonException)
            {
                issues.Add(new ValidationIssue(
                    $"line[{index + 1}]",
                    InvalidJsonMessage));
            }
        }

        return issues.Count == 0
            ? InputResult<IReadOnlyList<PortableActivityRecord>>.Success(records)
            : new InputResult<IReadOnlyList<PortableActivityRecord>>(records, issues);
    }

    private static PortableActivityRecord? ParseRecord(
        JsonElement element,
        ICollection<ValidationIssue> issues)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            issues.Add(new ValidationIssue("record", "Value must be a JSON object."));
            return null;
        }

        var properties = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!KnownFields.Contains(property.Name))
            {
                issues.Add(new ValidationIssue(property.Name, "Unknown property."));
                continue;
            }

            if (!properties.TryAdd(property.Name, property.Value))
            {
                issues.Add(new ValidationIssue(property.Name, "Duplicate property."));
            }
        }

        var id = ReadPositiveInteger(properties, "id", issues);
        var createdAt = ReadUtcDateTime(properties, "created_at", issues);
        var updatedAt = ReadUtcDateTime(properties, "updated_at", issues);
        var version = ReadPositiveInteger(properties, "version", issues);
        var type = ReadRequiredString(properties, "type", issues);
        var title = ReadRequiredString(properties, "title", issues);
        var description = ReadRequiredString(properties, "description", issues);
        var status = ReadRequiredString(properties, "status", issues);
        var source = ReadRequiredString(properties, "source", issues);
        var area = ReadRequiredString(properties, "area", issues);
        var result = ReadRequiredString(properties, "result", issues);
        var impact = ReadRequiredString(properties, "impact", issues);
        var effort = ReadRequiredString(properties, "effort", issues);
        var reference = ReadNullableString(properties, "reference", issues);
        var metadata = ReadMetadata(properties, "metadata", issues);

        return issues.Count == 0
            ? new PortableActivityRecord(
                id!.Value,
                createdAt!.Value,
                updatedAt!.Value,
                version!.Value,
                type!,
                title!,
                description!,
                status!,
                source!,
                area!,
                result!,
                impact!,
                effort!,
                reference,
                metadata)
            : null;
    }

    private static long? ReadPositiveInteger(
        IReadOnlyDictionary<string, JsonElement> properties,
        string name,
        ICollection<ValidationIssue> issues)
    {
        if (!properties.TryGetValue(name, out var value))
        {
            issues.Add(new ValidationIssue(name, "Field is required."));
            return null;
        }

        if (value.ValueKind != JsonValueKind.Number ||
            !value.TryGetInt64(out var number) ||
            number <= 0)
        {
            issues.Add(new ValidationIssue(name, "Value must be a positive integer."));
            return null;
        }

        return number;
    }

    private static DateTime? ReadUtcDateTime(
        IReadOnlyDictionary<string, JsonElement> properties,
        string name,
        ICollection<ValidationIssue> issues)
    {
        if (!properties.TryGetValue(name, out var value))
        {
            issues.Add(new ValidationIssue(name, "Field is required."));
            return null;
        }

        if (value.ValueKind != JsonValueKind.String ||
            !ActivityTimestamp.TryParseUtc(value.GetString(), out var timestamp))
        {
            issues.Add(new ValidationIssue(
                name,
                "Value must be a UTC ISO 8601 datetime with a Z offset."));
            return null;
        }

        return timestamp;
    }

    private static string? ReadRequiredString(
        IReadOnlyDictionary<string, JsonElement> properties,
        string name,
        ICollection<ValidationIssue> issues)
    {
        if (!properties.TryGetValue(name, out var value))
        {
            issues.Add(new ValidationIssue(name, "Field is required."));
            return null;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            issues.Add(new ValidationIssue(name, "Value must be a string."));
            return null;
        }

        return value.GetString();
    }

    private static string? ReadNullableString(
        IReadOnlyDictionary<string, JsonElement> properties,
        string name,
        ICollection<ValidationIssue> issues)
    {
        if (!properties.TryGetValue(name, out var value))
        {
            issues.Add(new ValidationIssue(name, "Field is required."));
            return null;
        }

        if (value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            issues.Add(new ValidationIssue(name, "Value must be a string or null."));
            return null;
        }

        return value.GetString();
    }

    private static string? ReadMetadata(
        IReadOnlyDictionary<string, JsonElement> properties,
        string name,
        ICollection<ValidationIssue> issues)
    {
        if (!properties.TryGetValue(name, out var value))
        {
            issues.Add(new ValidationIssue(name, "Field is required."));
            return null;
        }

        if (value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.Object)
        {
            issues.Add(new ValidationIssue(name, "Value must be a JSON object or null."));
            return null;
        }

        return value.GetRawText();
    }

    private static ValidationIssue Prefix(int index, ValidationIssue issue) =>
        new($"[{index}].{issue.Field}", issue.Message);

    private static InputResult<IReadOnlyList<PortableActivityRecord>> Invalid(
        params ValidationIssue[] issues) =>
        new([], issues);
}
