namespace Dbox.Activities;

public static class ActivityValidator
{
    public static ValidationResult ValidateCreate(ActivityCreateInput input)
    {
        var issues = new List<ValidationIssue>();

        ValidateEnum(input.Type, input.TypeProvided, ActivitySchema.Field("type"), issues);
        ValidateTitle(input.Title, input.TitleProvided, issues);

        ValidateEnum(input.Status, input.StatusProvided, ActivitySchema.Field("status"), issues, requiredWhenMissing: false);

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
            ValidateEnum(input.Type, provided: true, ActivitySchema.Field("type"), issues);
        }

        if (input.TitleProvided)
        {
            ValidateTitle(input.Title, provided: true, issues);
        }

        if (input.StatusProvided)
        {
            ValidateEnum(input.Status, provided: true, ActivitySchema.Field("status"), issues, requiredWhenMissing: false);
        }

        return new ValidationResult(issues);
    }

    private static void ValidateEnum(
        string? value,
        bool provided,
        ActivityFieldDefinition field,
        ICollection<ValidationIssue> issues,
        bool requiredWhenMissing = true)
    {
        if ((!provided && requiredWhenMissing) || string.IsNullOrEmpty(value))
        {
            if (requiredWhenMissing || provided)
            {
                issues.Add(new ValidationIssue(field.Name, "Field is required."));
            }

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
}
