namespace FileCollector.Processing;

internal sealed record FileTransfer(string SourcePath, string DestinationPath);

internal sealed record FileCollectionExecutionPlan(
    string ReadRoot,
    string WriteRoot,
    IReadOnlyList<FileTransfer> Transfers
);
