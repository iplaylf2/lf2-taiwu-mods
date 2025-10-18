namespace FileCollector.Processing;

internal sealed record FileTransferRecord(string SourcePath, string DestinationPath, DateTimeOffset Timestamp);
