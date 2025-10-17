using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileCollector;
using FileCollector.Configurations;
using FileCollector.IO;
using FileCollector.Processing;

return await FileCollectorCli.InvokeAsync(args);

internal static class FileCollectorCli
{
    private const string DefaultConfigurationFileName = "game-assets.yaml";

    public static Task<int> InvokeAsync(string[] args)
    {
        var command = BuildCommand();
        var parserConfiguration = new ParserConfiguration();
        var parseResult = command.Parse(args, parserConfiguration);
        var invocationConfiguration = parseResult.InvocationConfiguration ?? new InvocationConfiguration();
        return parseResult.InvokeAsync(invocationConfiguration, CancellationToken.None);
    }

    private static RootCommand BuildCommand()
    {
        var readWorkingDirectoryArgument = new Argument<string>("read-working-directory");
        readWorkingDirectoryArgument.HelpName = "Read working directory";

        var writeWorkingDirectoryArgument = new Argument<string>("write-working-directory");
        writeWorkingDirectoryArgument.HelpName = "Write working directory";

        var configurationOption = new Option<string>("--config", new[] { "-c" });
        configurationOption.Description = "Specify the YAML configuration file path. Defaults to game-assets.yaml in the current directory.";
        configurationOption.Arity = ArgumentArity.ZeroOrOne;

        var rootCommand = new RootCommand("Copy files into the write working directory according to the configuration plan.")
        {
            readWorkingDirectoryArgument,
            writeWorkingDirectoryArgument,
            configurationOption,
        };

        rootCommand.SetAction(parseResult =>
        {
            var readDir = parseResult.GetRequiredValue(readWorkingDirectoryArgument);
            var writeDir = parseResult.GetRequiredValue(writeWorkingDirectoryArgument);
            var configPath = parseResult.GetValue(configurationOption) ?? DefaultConfigurationFileName;
            return Execute(readDir, writeDir, configPath);
        });

        return rootCommand;
    }

    private static int Execute(string readWorkingDirectory, string writeWorkingDirectory, string configurationPath)
    {
        try
        {
            var fileSystem = new PhysicalFileSystem();

            var fullConfigurationPath = Path.GetFullPath(configurationPath);
            if (!fileSystem.FileExists(fullConfigurationPath))
            {
                throw new FileCollectionConfigurationException($"Configuration file not found: {fullConfigurationPath}");
            }

            var parser = new YamlFileCollectionPlanParser();
            FileCollectionPlan plan;
            using (var stream = fileSystem.OpenRead(fullConfigurationPath))
            using (var reader = new StreamReader(stream))
            {
                plan = parser.Parse(reader);
            }

            var executor = new FileCollectionExecutor(fileSystem);
            var result = executor.Execute(plan, readWorkingDirectory, writeWorkingDirectory);

            Console.Out.WriteLine($"Copied {result.CompletedTransfers.Count} files.");
            foreach (var transfer in result.CompletedTransfers)
            {
                Console.Out.WriteLine($"  {transfer.SourcePath} -> {transfer.DestinationPath}");
            }

            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
        catch (FileCollectionException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 2;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An unexpected error occurred: {ex}");
            return 3;
        }
    }
}
