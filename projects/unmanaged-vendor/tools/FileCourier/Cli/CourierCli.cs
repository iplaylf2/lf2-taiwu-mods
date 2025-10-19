using System.CommandLine;

namespace FileCourier.Cli;

internal static class CourierCli
{
    private const string DefaultManifestFileName = "manifest.yaml";

    public static Task<int> InvokeAsync(string[] args, Func<CliRequest, Task<int>> commandHandler)
    {
        var command = BuildCommand(commandHandler);
        var parserConfiguration = new ParserConfiguration();
        var parseResult = command.Parse(args, parserConfiguration);
        var invocationConfiguration = parseResult.InvocationConfiguration;

        return parseResult.InvokeAsync(invocationConfiguration);
    }

    private static RootCommand BuildCommand(Func<CliRequest, Task<int>> commandHandler)
    {
        var readWorkingDirectoryArgument = new Argument<string>("read-working-directory")
        {
            HelpName = "Read working directory"
        };

        var writeWorkingDirectoryArgument = new Argument<string>("write-working-directory")
        {
            HelpName = "Write working directory"
        };

        var manifestOption = new Option<string>("--manifest", ["-m"])
        {
            Description = $"Specify the YAML manifest file path. Defaults to {DefaultManifestFileName} in the current directory.",
            Arity = ArgumentArity.ZeroOrOne
        };

        var rootCommand = new RootCommand("Copy files into the write working directory according to the manifest plan.")
        {
            readWorkingDirectoryArgument,
            writeWorkingDirectoryArgument,
            manifestOption,
        };

        rootCommand.SetAction(async parseResult =>
        {
            var readDir = parseResult.GetRequiredValue(readWorkingDirectoryArgument);
            var writeDir = parseResult.GetRequiredValue(writeWorkingDirectoryArgument);
            var manifestPath = parseResult.GetValue(manifestOption) ?? DefaultManifestFileName;

            var request = new CliRequest(readDir, writeDir, manifestPath);

            return await commandHandler(request);
        });

        return rootCommand;
    }
}
