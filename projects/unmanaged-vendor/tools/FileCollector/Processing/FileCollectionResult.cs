using System.Collections.ObjectModel;

namespace FileCollector.Processing;

internal sealed class FileCollectionResult
{
    public FileCollectionResult(IEnumerable<FileTransferRecord> completedTransfers)
    {
        ArgumentNullException.ThrowIfNull(completedTransfers);

        CompletedTransfers = new ReadOnlyCollection<FileTransferRecord>([.. completedTransfers]);
    }

    public IReadOnlyList<FileTransferRecord> CompletedTransfers { get; }
}
