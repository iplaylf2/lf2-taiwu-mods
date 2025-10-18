namespace FileCourier.Manifest;

internal sealed record FileCourierEntry(string TargetDirectory, IReadOnlyList<string> SourceFiles);
