namespace Dbox.Activities;

public sealed class ActivityUpdateInput
{
    public string? Type { get; init; }

    public string? Title { get; init; }

    public string? Description { get; init; }

    public string? Status { get; init; }

    public bool TypeProvided { get; init; }

    public bool TitleProvided { get; init; }

    public bool DescriptionProvided { get; init; }

    public bool StatusProvided { get; init; }

    public bool HasChanges => TypeProvided || TitleProvided || DescriptionProvided || StatusProvided;
}
