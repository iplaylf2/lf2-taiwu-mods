using FileCollector.Configurations;

namespace FileCollector.Processing;

internal static class FileCollectionExecutor
{
    private sealed record PendingTransfer(string SourcePath, string TargetDirectory, string DestinationPath);

    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    public static void ValidateSourceFiles(FileCollectionPlan plan, string readWorkingDirectory)
    {
        var readRoot = NormalizeRoot(readWorkingDirectory, mustExist: true);
        var missingFiles = new List<string>();

        foreach (var entry in plan.Entries)
        {
            foreach (var sourceFile in entry.SourceFiles)
            {
                ValidateRelativePath(sourceFile, FileCollectionFields.SourceFiles);

                var sourceFilePath = ResolveRelativePath(readRoot, sourceFile);
                EnsureWithinRootOrThrow(sourceFilePath, readRoot, FileCollectionFields.SourceFiles);

                if (!File.Exists(sourceFilePath))
                {
                    missingFiles.Add(sourceFilePath);
                }
            }
        }

        if (missingFiles.Count > 0)
        {
            var details = string.Join(Environment.NewLine, missingFiles.Select(path => $"  {path}"));
            var message = "Source files not found:" + Environment.NewLine + details;
            throw new FileCollectionExecutionException(message);
        }
    }

    public static string PrepareWriteDirectory(string writeWorkingDirectory)
    {
        var normalized = NormalizeRoot(writeWorkingDirectory, mustExist: false);

        if (Directory.Exists(normalized))
        {
            if (Directory.EnumerateFileSystemEntries(normalized).Any())
            {
                throw new FileCollectionExecutionException($"Write working directory {normalized} must be empty.");
            }
        }
        else
        {
            _ = Directory.CreateDirectory(normalized);
        }

        _ = Directory.CreateDirectory(normalized);
        return normalized;
    }

    public static IReadOnlyList<(string SourcePath, string DestinationPath)> Execute
    (
        FileCollectionPlan plan,
        string readWorkingDirectory,
        string writeWorkingDirectory
    )
    {
        var readRoot = NormalizeRoot(readWorkingDirectory, mustExist: true);
        var writeRoot = NormalizeRoot(writeWorkingDirectory, mustExist: false);
        var pendingTransfers = BuildTransferPlan(plan, readRoot, writeRoot);

        return ExecuteTransfers(pendingTransfers, writeRoot);
    }

    private static List<PendingTransfer> BuildTransferPlan(FileCollectionPlan plan, string readRoot, string writeRoot)
    {
        var transfers = new List<PendingTransfer>();

        foreach (var entry in plan.Entries)
        {
            ValidateRelativePath(entry.TargetDirectory, FileCollectionFields.TargetDirectory);

            var targetDirectoryPath = ResolveRelativePath(writeRoot, entry.TargetDirectory);
            EnsureWithinRootOrThrow(targetDirectoryPath, writeRoot, FileCollectionFields.TargetDirectory);

            foreach (var sourceFile in entry.SourceFiles)
            {
                ValidateRelativePath(sourceFile, FileCollectionFields.SourceFiles);

                var sourceFilePath = ResolveRelativePath(readRoot, sourceFile);
                EnsureWithinRootOrThrow(sourceFilePath, readRoot, FileCollectionFields.SourceFiles);

                if (!File.Exists(sourceFilePath))
                {
                    throw new FileCollectionExecutionException($"Source file not found: {sourceFilePath}");
                }

                var fileName = Path.GetFileName(sourceFilePath);
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new FileCollectionExecutionException($"Source file path is missing a file name: {sourceFilePath}");
                }

                var destinationPath = Path.Combine(targetDirectoryPath, fileName);
                transfers.Add(new PendingTransfer(sourceFilePath, targetDirectoryPath, destinationPath));
            }
        }

        return transfers;
    }

    private static IReadOnlyList<(string SourcePath, string DestinationPath)> ExecuteTransfers
    (
        IReadOnlyList<PendingTransfer> transfers,
        string writeRoot
    )
    {
        var ensuredDirectories = new HashSet<string>(PathComparer) { writeRoot };
        var completedTransfers = new List<(string SourcePath, string DestinationPath)>(transfers.Count);

        foreach (var transfer in transfers)
        {
            if (ensuredDirectories.Add(transfer.TargetDirectory))
            {
                _ = Directory.CreateDirectory(transfer.TargetDirectory);
            }

            File.Copy(transfer.SourcePath, transfer.DestinationPath, overwrite: true);
            completedTransfers.Add((transfer.SourcePath, transfer.DestinationPath));
        }

        return [.. completedTransfers];
    }

    private static string NormalizeRoot(string root, bool mustExist)
    {
        var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));

        return mustExist && !Directory.Exists(fullPath)
            ? throw new FileCollectionExecutionException($"Directory does not exist: {fullPath}")
            : fullPath;
    }

    private static string ResolveRelativePath(string root, string relativePath)
    {
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.Join(root, relativePath)));
    }

    private static void EnsureWithinRootOrThrow(string candidatePath, string rootPath, string configFieldName)
    {
        var normalizedRoot = AppendDirectorySeparator(rootPath);
        var normalizedCandidate = Path.TrimEndingDirectorySeparator(candidatePath);

        if
        (
            !normalizedCandidate.Equals(normalizedRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), PathComparison) &&
            !normalizedCandidate.StartsWith(normalizedRoot, PathComparison)
        )
        {
            throw new FileCollectionExecutionException
            (
                $"Configuration field {configFieldName} points to {candidatePath}, which lies outside the write root {rootPath}."
            );
        }
    }

    private static void ValidateRelativePath(string path, string fieldName)
    {
        _ = Path.IsPathRooted(path) || Path.IsPathFullyQualified(path)
            ? throw new FileCollectionExecutionException($"Configuration field {fieldName} value {path} must be a relative path.")
            : (object?)null;
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
