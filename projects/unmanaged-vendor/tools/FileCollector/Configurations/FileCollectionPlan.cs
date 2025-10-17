using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FileCollector.Configurations;

/// <summary>
/// Represents the complete set of file collection entries described by the configuration.
/// </summary>
public sealed class FileCollectionPlan
{
    public FileCollectionPlan(IEnumerable<FileCollectionEntry> entries)
    {
        Entries = new ReadOnlyCollection<FileCollectionEntry>(MaterializeEntries(entries));
    }

    public IReadOnlyList<FileCollectionEntry> Entries { get; }

    private static List<FileCollectionEntry> MaterializeEntries(IEnumerable<FileCollectionEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        var materialized = new List<FileCollectionEntry>();
        foreach (var entry in entries)
        {
            materialized.Add(entry ?? throw new ArgumentException("Entry cannot be null.", nameof(entries)));
        }

        if (materialized.Count == 0)
        {
            throw new ArgumentException("The collection plan must contain at least one entry.", nameof(entries));
        }

        return materialized;
    }
}
