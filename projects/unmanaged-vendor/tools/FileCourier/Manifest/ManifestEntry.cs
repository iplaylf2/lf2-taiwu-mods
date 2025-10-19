namespace FileCourier.Manifest;

internal sealed record ManifestEntry(string TargetDirectory, IReadOnlyList<string> SourceFiles);
