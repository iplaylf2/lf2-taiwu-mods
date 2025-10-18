using System.Collections.ObjectModel;

namespace FileCollector.Configurations;


internal sealed class FileCollectionPlan(IEnumerable<FileCollectionEntry> entries)
{
    public IReadOnlyList<FileCollectionEntry> Entries { get; } = new ReadOnlyCollection<FileCollectionEntry>(MaterializeEntries(entries));

    private static List<FileCollectionEntry> MaterializeEntries(IEnumerable<FileCollectionEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var materialized = new List<FileCollectionEntry>();
        foreach (var entry in entries)
        {
            materialized.Add(entry ?? throw new ArgumentException("Entry cannot be null.", nameof(entries)));
        }

        return materialized.Count == 0
            ? throw new ArgumentException("The collection plan must contain at least one entry.", nameof(entries))
            : materialized;
    }
}
