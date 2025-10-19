namespace FileCourier.Processing;

internal sealed record FileTransfer(string SourcePath, string DestinationPath);

internal sealed record ExecutionPlan(
    string ReadRoot,
    string WriteRoot,
    IReadOnlyList<FileTransfer> Transfers
);
