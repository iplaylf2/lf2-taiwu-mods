namespace FileCourier.Processing;

internal static class FileCourierPlanExecutor
{
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    public static Task<IReadOnlyList<FileTransfer>> ExecuteAsync(FileCourierExecutionPlan plan)
    {
        EnsureWriteDirectory(plan.WriteRoot);
        return ExecuteTransfersAsync(plan.Transfers);
    }

    private static void EnsureWriteDirectory(string writeRoot)
    {
        if (!Directory.Exists(writeRoot))
        {
            _ = Directory.CreateDirectory(writeRoot);
        }
    }

    private static async Task<IReadOnlyList<FileTransfer>> ExecuteTransfersAsync(IReadOnlyList<FileTransfer> transfers)
    {
        var ensuredDirectories = new HashSet<string>(PathComparer);
        var completedTransfers = new List<FileTransfer>(transfers.Count);

        foreach (var transfer in transfers)
        {
            var targetDirectory = Path.GetDirectoryName(transfer.DestinationPath);
            if (string.IsNullOrEmpty(targetDirectory))
            {
                throw new FileCourierExecutionException($"Destination path {transfer.DestinationPath} is missing a directory segment.");
            }

            if (ensuredDirectories.Add(targetDirectory))
            {
                _ = Directory.CreateDirectory(targetDirectory);
            }

            await using var sourceStream = new FileStream
            (
                transfer.SourcePath,
                new FileStreamOptions
                {
                    Mode = FileMode.Open,
                    Access = FileAccess.Read,
                    Share = FileShare.Read,
                    Options = FileOptions.Asynchronous | FileOptions.SequentialScan
                }
            );

            await using var destinationStream = new FileStream
            (
                transfer.DestinationPath,
                new FileStreamOptions
                {
                    Mode = FileMode.Create,
                    Access = FileAccess.Write,
                    Share = FileShare.None,
                    Options = FileOptions.Asynchronous
                }
            );

            await sourceStream.CopyToAsync(destinationStream);
            completedTransfers.Add(transfer);
        }

        return completedTransfers;
    }
}
