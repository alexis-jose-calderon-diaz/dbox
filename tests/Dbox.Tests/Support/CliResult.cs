namespace Dbox.Tests.Support;

public sealed record CliResult(int ExitCode, string Output, string Error);
