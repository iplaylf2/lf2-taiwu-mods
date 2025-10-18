namespace FileCollector.Configurations;

internal sealed record FileCollectionEntry(string TargetDirectory, IReadOnlyList<string> SourceFiles);
