namespace Dbox.Activities;

public static class ActivityValidator
{
    public static ValidationResult ValidateCreate(ActivityCreateInput input)
    {
        var issues = new List<ValidationIssue>();

        ValidateString(input.Type, input.TypeProvided, ActivitySchema.Field("type"), issues);
        ValidateTitle(input.Title, input.TitleProvided, issues);
        ValidateEnum(input.Status, input.StatusProvided, ActivitySchema.Field("status"), issues);
        ValidateString(input.Description, input.DescriptionProvided, ActivitySchema.Field("description"), issues);
        ValidateString(input.Source, input.SourceProvided, ActivitySchema.Field("source"), issues);
        ValidateString(input.Area, input.AreaProvided, ActivitySchema.Field("area"), issues);
        ValidateString(input.Result, input.ResultProvided, ActivitySchema.Field("result"), issues);
        ValidateString(input.Impact, input.ImpactProvided, ActivitySchema.Field("impact"), issues);
        ValidateEnum(input.Effort, input.EffortProvided, ActivitySchema.Field("effort"), issues);

        return new ValidationResult(issues);
    }

    public static ValidationResult ValidateUpdate(ActivityUpdateInput input)
    {
        var issues = new List<ValidationIssue>();

        if (!input.HasChanges)
        {
            issues.Add(new ValidationIssue("update", "At least one writable field is required."));
        }

        if (input.TypeProvided)
        {
            ValidateString(input.Type, provided: true, ActivitySchema.Field("type"), issues);
        }

        if (input.TitleProvided)
        {
            ValidateTitle(input.Title, provided: true, issues);
        }

        if (input.DescriptionProvided)
        {
            ValidateString(input.Description, provided: true, ActivitySchema.Field("description"), issues);
        }

        if (input.StatusProvided)
        {
            ValidateEnum(input.Status, provided: true, ActivitySchema.Field("status"), issues);
        }

        if (input.SourceProvided)
        {
            ValidateString(input.Source, provided: true, ActivitySchema.Field("source"), issues);
        }

        if (input.AreaProvided)
        {
            ValidateString(input.Area, provided: true, ActivitySchema.Field("area"), issues);
        }

        if (input.ResultProvided)
        {
            ValidateString(input.Result, provided: true, ActivitySchema.Field("result"), issues);
        }

        if (input.ImpactProvided)
        {
            ValidateString(input.Impact, provided: true, ActivitySchema.Field("impact"), issues);
        }

        if (input.EffortProvided)
        {
            ValidateEnum(input.Effort, provided: true, ActivitySchema.Field("effort"), issues);
        }

        return new ValidationResult(issues);
    }

    public static ValidationResult ValidateFilter(ActivityFilter filter)
    {
        var issues = new List<ValidationIssue>();

        ValidateFilterString(filter.Type, "type", issues);

        if (filter.Status is not null && !ActivitySchema.IsStatus(filter.Status))
        {
            issues.Add(new ValidationIssue("status", $"Value must be one of: {string.Join(", ", ActivitySchema.Statuses)}."));
        }

        ValidateFilterString(filter.Area, "area", issues);
        ValidateFilterString(filter.Source, "source", issues);

        if (filter.Effort is not null && !ActivitySchema.IsEffort(filter.Effort))
        {
            issues.Add(new ValidationIssue("effort", $"Value must be one of: {string.Join(", ", ActivitySchema.Efforts)}."));
        }

        ValidateFilterSearch(filter.Title, "title", issues);
        ValidateFilterSearch(filter.Description, "description", issues);

        if (filter.CreatedFrom is not null && filter.CreatedTo is not null &&
            filter.CreatedFrom > filter.CreatedTo)
        {
            issues.Add(new ValidationIssue("created_from", "Value must not be later than created_to."));
        }

        return new ValidationResult(issues);
    }

    private static void ValidateFilterString(
        string? value,
        string field,
        ICollection<ValidationIssue> issues)
    {
        if (value is not null && string.IsNullOrWhiteSpace(value))
        {
            issues.Add(new ValidationIssue(field, "Field must be a non-blank value."));
        }
    }

    private static void ValidateFilterSearch(
        string? value,
        string field,
        ICollection<ValidationIssue> issues)
    {
        ValidateFilterString(value, field, issues);
    }

    private static void ValidateString(
        string? value,
        bool provided,
        ActivityFieldDefinition field,
        ICollection<ValidationIssue> issues)
    {
        if (!provided || value is null || (field.NonBlank && string.IsNullOrWhiteSpace(value)))
        {
            issues.Add(new ValidationIssue(field.Name, "Field must be a non-blank value."));
            return;
        }

        if (field.MaxLength is { } maxLength && value.Length > maxLength)
        {
            issues.Add(new ValidationIssue(
                field.Name,
                $"Value must be at most {maxLength} characters."));
        }
    }

    private static void ValidateEnum(
        string? value,
        bool provided,
        ActivityFieldDefinition field,
        ICollection<ValidationIssue> issues)
    {
        if (!provided || value is null || (field.NonBlank && string.IsNullOrWhiteSpace(value)))
        {
            issues.Add(new ValidationIssue(field.Name, "Field must be a non-blank value."));
            return;
        }

        if (field.EnumValues is null || !field.EnumValues.Contains(value, StringComparer.Ordinal))
        {
            issues.Add(new ValidationIssue(
                field.Name,
                $"Value must be one of: {string.Join(", ", field.EnumValues ?? [])}."));
        }
    }

    private static void ValidateTitle(string? value, bool provided, ICollection<ValidationIssue> issues)
    {
        var field = ActivitySchema.Field("title");
        ValidateString(value, provided, field, issues);
    }
}
