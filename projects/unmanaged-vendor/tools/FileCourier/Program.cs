using System.CommandLine;
using FileCourier.Cli;
using FileCourier.Manifest;
using FileCourier.Processing;

return await FileCourier.FileCourierCli.InvokeAsync(args);

namespace FileCourier
{
    internal static class FileCourierCli
    {
        private const string DefaultConfigurationFileName = "manifest.yaml";

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
                var manifest = FileCourierConfigurationLoader.Load(cliContext.ConfigurationPath);
                var executionPlan = FileCourierExecutionPlanBuilder.Build(manifest, cliContext.ReadRoot, cliContext.WriteRoot);

                var totalTransfers = executionPlan.Transfers.Count;
                var completedTransfers = 0;

                await foreach (var transfer in FileCourierPlanExecutor.ExecuteAsync(executionPlan))
                {
                    completedTransfers++;
                    await Console.Out.WriteLineAsync
                    (
                        $"[{completedTransfers}/{totalTransfers}] {transfer.SourcePath} -> {transfer.DestinationPath}"
                    );
                }

                await Console.Out.WriteLineAsync($"Copied {completedTransfers} files.");

                return 0;
            }
            catch (ArgumentException ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
                return 1;
            }
            catch (FileCourierException ex)
            {
                await Console.Error.WriteLineAsync(ex.ToDisplayString());
                return 2;
            }
        }
    }
}
