using FileCourier.Manifest;

namespace FileCourier.Processing;

internal static class ExecutionPlanBuilder
{
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    public static ExecutionPlan Build(ManifestPlan plan, string readWorkingDirectory, string writeWorkingDirectory)
    {
        var readRoot = NormalizeRoot(readWorkingDirectory);
        var writeRoot = NormalizeRoot(writeWorkingDirectory);

        var invalidTargetEntries = new List<string>();
        var invalidSourceEntries = new List<string>();
        var missingFiles = new List<string>();
        var transfers = new List<FileTransfer>();

        foreach (var entry in plan.Entries)
        {
            var targetValidationError = ValidateRelativePath(entry.TargetDirectory, ManifestFields.TargetDirectory);
            if (targetValidationError is { })
            {
                invalidTargetEntries.Add(targetValidationError);
                continue;
            }

            var targetDirectoryPath = ResolveRelativePath(writeRoot, entry.TargetDirectory);
            var targetError = ValidateWithinRoot(targetDirectoryPath, writeRoot, ManifestFields.TargetDirectory);
            if (targetError is { })
            {
                invalidTargetEntries.Add(targetError);
                continue;
            }

            foreach (var sourceFile in entry.SourceFiles)
            {
                var sourceValidationError = ValidateRelativePath(sourceFile, ManifestFields.SourceFiles);
                if (sourceValidationError is { })
                {
                    invalidSourceEntries.Add(sourceValidationError);
                    continue;
                }

                var sourceFilePath = ResolveRelativePath(readRoot, sourceFile);
                var sourceError = ValidateWithinRoot(sourceFilePath, readRoot, ManifestFields.SourceFiles);
                if (sourceError is { })
                {
                    invalidSourceEntries.Add(sourceError);
                    continue;
                }

                if (Directory.Exists(sourceFilePath) && !File.Exists(sourceFilePath))
                {
                    invalidSourceEntries.Add($"Source path points to a directory: {sourceFilePath}");
                    continue;
                }

                if (!File.Exists(sourceFilePath))
                {
                    missingFiles.Add(sourceFilePath);
                    continue;
                }

                var fileName = Path.GetFileName(sourceFilePath);
                if (string.IsNullOrEmpty(fileName))
                {
                    invalidSourceEntries.Add($"Source file path is missing a file name: {sourceFilePath}");
                    continue;
                }

                var destinationPath = Path.Combine(targetDirectoryPath, fileName);
                transfers.Add(new FileTransfer(sourceFilePath, destinationPath));
            }
        }

        if (invalidTargetEntries.Count > 0 || invalidSourceEntries.Count > 0 || missingFiles.Count > 0)
        {
            var lines = new List<string>();

            if (invalidTargetEntries.Count > 0)
            {
                lines.Add("Invalid target entries:");
                lines.AddRange(invalidTargetEntries.OrderBy(message => message, StringComparer.Ordinal).Select(message => $"  {message}"));
            }

            if (invalidSourceEntries.Count > 0)
            {
                lines.Add("Invalid source entries:");
                lines.AddRange(invalidSourceEntries.OrderBy(message => message, StringComparer.Ordinal).Select(path => $"  {path}"));
            }

            if (missingFiles.Count > 0)
            {
                lines.Add("Source files not found:");
                lines.AddRange(missingFiles.OrderBy(path => path, PathComparer).Select(path => $"  {path}"));
            }

            throw new ExecutionException(string.Join(Environment.NewLine, lines));
        }

        return new ExecutionPlan(readRoot, writeRoot, transfers);
    }

    private static string NormalizeRoot(string root)
    {
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
    }

    private static string ResolveRelativePath(string root, string relativePath)
    {
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.Join(root, relativePath)));
    }

    private static string? ValidateWithinRoot(string candidatePath, string rootPath, string manifestFieldName)
    {
        var normalizedRoot = AppendDirectorySeparator(rootPath);
        var normalizedCandidate = Path.TrimEndingDirectorySeparator(candidatePath);

        return normalizedCandidate.Equals(normalizedRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), PathComparison) ||
               normalizedCandidate.StartsWith(normalizedRoot, PathComparison)
            ? null
            : $"Manifest field {manifestFieldName} points to {candidatePath}, which lies outside the root directory {rootPath}.";
    }

    private static string? ValidateRelativePath(string path, string fieldName)
    {
        return Path.IsPathRooted(path) || Path.IsPathFullyQualified(path)
            ? $"Manifest field {fieldName} value {path} must be a relative path."
            : null;
    }

    private static string AppendDirectorySeparator(string path)
    {
        return path switch
        {
            var p when string.IsNullOrEmpty(p) => path,
            var p when Path.EndsInDirectorySeparator(p) => path,
            var p => p + Path.DirectorySeparatorChar
        };
    }
}
