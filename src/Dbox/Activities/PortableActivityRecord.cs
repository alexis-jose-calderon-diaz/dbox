namespace Dbox.Activities;

public sealed record PortableActivityRecord(
    long Id,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    long Version,
    string Type,
    string Title,
    string Description,
    string Status,
    string Source,
    string Area,
    string Result,
    string Impact,
    string Effort,
    string? Reference,
    string? Metadata);
