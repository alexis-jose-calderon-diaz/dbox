using Dbox.Cli;

return await DboxCli.InvokeAsync(
    args,
    Console.Out,
    Console.Error,
    Directory.GetCurrentDirectory());
