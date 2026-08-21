namespace Dbox.Cli;

public static class ExitCodes
{
    public const int Success = 0;
    public const int UnexpectedError = 1;
    public const int ValidationError = 2;
    public const int ResourceNotFound = 3;
    public const int ConflictError = ResourceNotFound;
    public const int DatabaseError = 4;
    public const int IoError = DatabaseError;
}
