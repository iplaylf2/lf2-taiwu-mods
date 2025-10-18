namespace FileCollector.Configurations;

internal sealed class FileCollectionPlan(IReadOnlyList<FileCollectionEntry> entries)
{
    public IReadOnlyList<FileCollectionEntry> Entries { get; } = [.. entries];
}
