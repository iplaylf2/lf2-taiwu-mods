using FileCollector.Configurations;
using FileCollector.Processing;

namespace FileCollector.Cli;

internal static class CliArgumentsAnalyzer
{
    internal sealed record Result(string ReadRoot, string WriteRoot, string ConfigurationPath);

    public static Result Analyze(string readWorkingDirectory, string writeWorkingDirectory, string configurationPath)
    {
        var readRoot = Normalize(readWorkingDirectory);
        if (!Directory.Exists(readRoot))
        {
            var message = $"Read working directory does not exist: {readRoot}";
            throw new FileCollectionExecutionException(message);
        }

        var writeRoot = Normalize(writeWorkingDirectory);
        if (Directory.Exists(writeRoot) && Directory.EnumerateFileSystemEntries(writeRoot).Any())
        {
            var message = $"Write working directory {writeRoot} must be empty.";
            throw new FileCollectionExecutionException(message);
        }

        var configurationFullPath = Normalize(configurationPath);
        if (!File.Exists(configurationFullPath))
        {
            var message = $"Configuration file not found: {configurationFullPath}";
            throw new FileCollectionConfigurationException(message);
        }

        return new Result(readRoot, writeRoot, configurationFullPath);
    }

    private static string Normalize(string path)
    {
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
    }
}
