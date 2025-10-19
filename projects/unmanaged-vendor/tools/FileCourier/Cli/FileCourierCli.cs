using System.CommandLine;

namespace FileCourier.Cli;

internal static class FileCourierCli
{
    private const string DefaultConfigurationFileName = "manifest.yaml";

    public static Task<int> InvokeAsync(string[] args, Func<FileCourierCliRequest, Task<int>> commandHandler)
    {
        var command = BuildCommand(commandHandler);
        var parserConfiguration = new ParserConfiguration();
        var parseResult = command.Parse(args, parserConfiguration);
        var invocationConfiguration = parseResult.InvocationConfiguration;

        return parseResult.InvokeAsync(invocationConfiguration);
    }

    private static RootCommand BuildCommand(Func<FileCourierCliRequest, Task<int>> commandHandler)
    {
        var readWorkingDirectoryArgument = new Argument<string>("read-working-directory")
        {
            HelpName = "Read working directory"
        };

        var writeWorkingDirectoryArgument = new Argument<string>("write-working-directory")
        {
            HelpName = "Write working directory"
        };

        var configurationOption = new Option<string>("--config", ["-c"])
        {
            Description = $"Specify the YAML configuration file path. Defaults to {DefaultConfigurationFileName} in the current directory.",
            Arity = ArgumentArity.ZeroOrOne
        };

        var rootCommand = new RootCommand("Copy files into the write working directory according to the configuration plan.")
        {
            readWorkingDirectoryArgument,
            writeWorkingDirectoryArgument,
            configurationOption,
        };

        rootCommand.SetAction(async parseResult =>
        {
            var readDir = parseResult.GetRequiredValue(readWorkingDirectoryArgument);
            var writeDir = parseResult.GetRequiredValue(writeWorkingDirectoryArgument);
            var configPath = parseResult.GetValue(configurationOption) ?? DefaultConfigurationFileName;

            var request = new FileCourierCliRequest(readDir, writeDir, configPath);

            return await commandHandler(request);
        });

        return rootCommand;
    }
}
