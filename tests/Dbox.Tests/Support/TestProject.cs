using System.Text;
using Dbox.Cli;

namespace Dbox.Tests.Support;

public sealed class TestProject : IDisposable
{
    public TestProject()
    {
        Root = Path.Combine(Path.GetTempPath(), "dbox-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Root);
    }

    public string Root { get; }

    public string CreateChild(string name = "child")
    {
        var path = Path.Combine(Root, name);
        Directory.CreateDirectory(path);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(Root))
        {
            Directory.Delete(Root, recursive: true);
        }
    }

    public static async Task<CliResult> RunAsync(string workingDirectory, params string[] args)
    {
        using var output = new StringWriter(new StringBuilder());
        using var error = new StringWriter(new StringBuilder());
        var exitCode = await DboxCli.InvokeAsync(args, output, error, workingDirectory);
        return new CliResult(exitCode, output.ToString(), error.ToString());
    }

    public static async Task<CliResult> RunWithInputAsync(
        string workingDirectory,
        string input,
        params string[] args)
    {
        using var output = new StringWriter(new StringBuilder());
        using var error = new StringWriter(new StringBuilder());
        using var inputReader = new StringReader(input);
        var exitCode = await DboxCli.InvokeAsync(
            args,
            output,
            error,
            workingDirectory,
            input: inputReader);
        return new CliResult(exitCode, output.ToString(), error.ToString());
    }
}
