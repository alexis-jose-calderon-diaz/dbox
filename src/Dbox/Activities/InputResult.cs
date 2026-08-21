namespace Dbox.Activities;

public sealed record InputResult<T>(
    T? Value,
    IReadOnlyList<ValidationIssue> Issues,
    string? ErrorMessage = null)
{
    public bool IsValid => Issues.Count == 0;

    public static InputResult<T> Success(T value) => new(value, []);

    public static InputResult<T> Failure(params ValidationIssue[] issues) => new(default, issues);
}
