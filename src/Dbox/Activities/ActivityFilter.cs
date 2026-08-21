namespace Dbox.Activities;

public sealed record ActivityFilter(
    string? Type,
    string? Status,
    string? Area,
    string? Source,
    string? Effort,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    string? Title,
    string? Description);
