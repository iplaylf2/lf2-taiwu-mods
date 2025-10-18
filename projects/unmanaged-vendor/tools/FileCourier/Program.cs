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

                var transfers = await FileCourierPlanExecutor.ExecuteAsync(executionPlan);

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
            catch (FileCourierException ex)
            {
                await Console.Error.WriteLineAsync(ex.ToDisplayString());
                return 2;
            }
        }
    }
}
