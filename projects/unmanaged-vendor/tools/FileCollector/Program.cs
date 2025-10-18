using System.CommandLine;
using FileCollector;
using FileCollector.Configurations;
using FileCollector.IO;
using FileCollector.Processing;

return await FileCollectorCli.InvokeAsync(args).ConfigureAwait(false);

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
            var invocationConfiguration = parseResult.InvocationConfiguration ?? new InvocationConfiguration();
            return parseResult.InvokeAsync(invocationConfiguration, CancellationToken.None);
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
                Description = "Specify the YAML configuration file path. Defaults to assets.yaml in the current directory.",
                Arity = ArgumentArity.ZeroOrOne
            };

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
        }
    }
}