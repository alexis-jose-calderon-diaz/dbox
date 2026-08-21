namespace Dbox.Activities;

public sealed class ActivityUpdateInput
{
    public long? Version { get; init; }

    public string? Type { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? Status { get; init; }

    public string? Source { get; init; }

    public string? Area { get; init; }

    public string? Result { get; init; }

    public string? Impact { get; init; }

    public string? Effort { get; init; }

    public string? Reference { get; init; }

    public string? Metadata { get; init; }

    public bool TypeProvided { get; init; }

    public bool TitleProvided { get; init; }

    public bool DescriptionProvided { get; init; }

    public bool StatusProvided { get; init; }

    public bool SourceProvided { get; init; }

    public bool AreaProvided { get; init; }

    public bool ResultProvided { get; init; }

    public bool ImpactProvided { get; init; }

    public bool EffortProvided { get; init; }

    public bool ReferenceProvided { get; init; }

    public bool MetadataProvided { get; init; }

    public bool VersionProvided { get; init; }

    public bool HasChanges => TypeProvided || TitleProvided || DescriptionProvided || StatusProvided ||
        SourceProvided || AreaProvided || ResultProvided || ImpactProvided || EffortProvided ||
        ReferenceProvided || MetadataProvided;
}
