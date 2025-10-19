using FileCourier.Manifest;
using FileCourier.Processing;

namespace FileCourier.Cli;

internal static class CliArgumentsAnalyzer
{
    internal sealed record Result(string ReadRoot, string WriteRoot, string ManifestPath);

    public static Result Analyze(string readWorkingDirectory, string writeWorkingDirectory, string manifestPath)
    {
        var readRoot = Normalize(readWorkingDirectory);
        if (!Directory.Exists(readRoot))
        {
            var message = $"Read working directory does not exist: {readRoot}";
            throw new ExecutionException(message);
        }

        var writeRoot = Normalize(writeWorkingDirectory);
        if (Directory.Exists(writeRoot) && Directory.EnumerateFileSystemEntries(writeRoot).Any())
        {
            var message = $"Write working directory {writeRoot} must be empty.";
            throw new ExecutionException(message);
        }

        var manifestFullPath = Normalize(manifestPath);
        if (!File.Exists(manifestFullPath))
        {
            var message = $"Manifest file not found: {manifestFullPath}";
            throw new ManifestException(message);
        }

        return new Result(readRoot, writeRoot, manifestFullPath);
    }

    private static string Normalize(string path)
    {
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
    }
}
