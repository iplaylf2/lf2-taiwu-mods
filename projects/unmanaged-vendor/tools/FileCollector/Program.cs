using System.CommandLine;
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
                var fullConfigurationPath = Path.GetFullPath(configurationPath);
                if (!File.Exists(fullConfigurationPath))
                {
                    throw new FileCollectionConfigurationException($"Configuration file not found: {fullConfigurationPath}");
                }

                await using var stream = File.OpenRead(fullConfigurationPath);
                using var reader = new StreamReader(stream);
                var parser = new YamlFileCollectionPlanParser();
                var plan = parser.Parse(reader);

                FileCollectionExecutor.ValidateSourceFiles(plan, readWorkingDirectory);

                var preparedWriteDirectory = FileCollectionExecutor.PrepareWriteDirectory(writeWorkingDirectory);
                var transfers = await FileCollectionExecutor.ExecuteAsync(plan, readWorkingDirectory, preparedWriteDirectory);

                await Console.Out.WriteLineAsync($"Copied {transfers.Count} files.");
                foreach (var (sourcePath, destinationPath) in transfers)
                {
                    await Console.Out.WriteLineAsync($"  {sourcePath} -> {destinationPath}");
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
