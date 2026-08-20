namespace Dbox.Activities;

public sealed record ActivityCreateInput(
    string? Type,
    string? Title,
    string? Description,
    string? Status,
    bool TypeProvided,
    bool TitleProvided,
    bool DescriptionProvided,
    bool StatusProvided);
