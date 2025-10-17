using System;

namespace FileCollector.Processing;

public sealed record FileTransferRecord(string SourcePath, string DestinationPath, DateTimeOffset Timestamp);
