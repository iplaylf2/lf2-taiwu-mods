using FileCollector.Configurations;
using FileCollector.IO;

namespace FileCollector.Processing;

internal sealed class FileCollectionExecutor(IFileSystem fileSystem, TimeProvider? timeProvider = null)
{
    private sealed record PendingTransfer(string SourcePath, string TargetDirectory, string DestinationPath);

    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    private readonly IFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    public FileCollectionResult Execute(FileCollectionPlan plan, string readWorkingDirectory, string writeWorkingDirectory)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentException.ThrowIfNullOrWhiteSpace(readWorkingDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(writeWorkingDirectory);

        var readRoot = NormalizeRoot(readWorkingDirectory, mustExist: true);
        var writeRoot = NormalizeRoot(writeWorkingDirectory, mustExist: false);

        EnsureWriteRootIsReady(writeRoot);

        var pendingTransfers = BuildTransferPlan(plan, readRoot, writeRoot);
        var completedTransfers = ExecuteTransfers(pendingTransfers, writeRoot);

        return new FileCollectionResult(completedTransfers);
    }

    private void EnsureWriteRootIsReady(string writeRoot)
    {
        if (_fileSystem.DirectoryExists(writeRoot))
        {
            if (_fileSystem.EnumerateFileSystemEntries(writeRoot).Any())
            {
                throw new FileCollectionExecutionException($"Write working directory {writeRoot} must be empty.");
            }
        }
        else
        {
            _fileSystem.EnsureDirectory(writeRoot);
        }

        _fileSystem.EnsureDirectory(writeRoot);
    }

    private List<PendingTransfer> BuildTransferPlan(FileCollectionPlan plan, string readRoot, string writeRoot)
    {
        var transfers = new List<PendingTransfer>();

        foreach (var entry in plan.Entries)
        {
            ValidateRelativePath(entry.TargetDirectory, "target-dir");
            var targetDirectoryPath = ResolveRelativePath(writeRoot, entry.TargetDirectory);
            EnsureWithinRootOrThrow(targetDirectoryPath, writeRoot, "target-dir");

            foreach (var sourceFile in entry.SourceFiles)
            {
                ValidateRelativePath(sourceFile, "source-files");

                var sourceFilePath = ResolveRelativePath(readRoot, sourceFile);
                EnsureWithinRootOrThrow(sourceFilePath, readRoot, "source-files");

                if (!_fileSystem.FileExists(sourceFilePath))
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

    private List<FileTransferRecord> ExecuteTransfers(IReadOnlyList<PendingTransfer> transfers, string writeRoot)
    {
        HashSet<string> ensuredDirectories = new(PathComparer) { writeRoot };
        var completedTransfers = new List<FileTransferRecord>(transfers.Count);

        foreach (var transfer in transfers)
        {
            if (ensuredDirectories.Add(transfer.TargetDirectory))
            {
                _fileSystem.EnsureDirectory(transfer.TargetDirectory);
            }

            _fileSystem.CopyFile(transfer.SourcePath, transfer.DestinationPath, overwrite: true);
            completedTransfers.Add(new FileTransferRecord(transfer.SourcePath, transfer.DestinationPath, _timeProvider.GetUtcNow()));
        }

        return completedTransfers;
    }

    private string NormalizeRoot(string root, bool mustExist)
    {
        var fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));

        return mustExist && !_fileSystem.DirectoryExists(fullPath)
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

        if (!normalizedCandidate.Equals(normalizedRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), PathComparison) &&
            !normalizedCandidate.StartsWith(normalizedRoot, PathComparison))
        {
            throw new FileCollectionExecutionException($"Configuration field {configFieldName} points to {candidatePath}, which lies outside the write root {rootPath}.");
        }
    }

    private static void ValidateRelativePath(string path, string fieldName)
    {
        if (Path.IsPathRooted(path) || Path.IsPathFullyQualified(path))
        {
            throw new FileCollectionExecutionException($"Configuration field {fieldName} value {path} must be a relative path.");
        }
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
