using System.CommandLine;
using FileCollector.Cli;
using FileCollector.Configurations;
using FileCollector.Processing;

return await FileCollector.FileCollectorCli.InvokeAsync(args);

namespace FileCollector
{
    internal static class FileCollectorCli
    {
        private const string DefaultConfigurationFileName = "assets.yaml";

        public static Task<int> InvokeAsync(string[] args)
        {
            var command = BuildCommand();
            var parserConfiguration = new ParserConfiguration();
            var parseResult = command.Parse(args, parserConfiguration);
            var invocationConfiguration = parseResult.InvocationConfiguration;

            return parseResult.InvokeAsync(invocationConfiguration);
        }

        private static RootCommand BuildCommand()
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

                return await ExecuteAsync(readDir, writeDir, configPath);
            });

            return rootCommand;
        }

        private static async Task<int> ExecuteAsync(string readWorkingDirectory, string writeWorkingDirectory, string configurationPath)
        {
            try
            {
                var cliContext = CliArgumentsAnalyzer.Analyze(readWorkingDirectory, writeWorkingDirectory, configurationPath);
                var plan = FileCollectionConfigurationLoader.Load(cliContext.ConfigurationPath);
                var executionPlan = FileCollectionExecutionPlanBuilder.Build(plan, cliContext.ReadRoot, cliContext.WriteRoot);

                var transfers = await FileCollectionPlanExecutor.ExecuteAsync(executionPlan);

                await Console.Out.WriteLineAsync($"Copied {transfers.Count} files.");
                foreach (var transfer in transfers)
                {
                    await Console.Out.WriteLineAsync($"  {transfer.SourcePath} -> {transfer.DestinationPath}");
                }

                return 0;
            }
            catch (ArgumentException ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
                return 1;
            }
            catch (FileCollectionException ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
                return 2;
            }
        }
    }
}
