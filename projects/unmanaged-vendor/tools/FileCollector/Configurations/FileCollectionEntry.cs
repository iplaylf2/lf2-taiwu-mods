using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FileCollector.Configurations;

/// <summary>
/// Represents a single target directory mapping to a set of source files.
/// </summary>
public sealed class FileCollectionEntry
{
    public FileCollectionEntry(string targetDirectory, IEnumerable<string> sourceFiles)
    {
        TargetDirectory = NormalizeDirectory(targetDirectory);
        SourceFiles = new ReadOnlyCollection<string>(BuildSourceList(sourceFiles));
    }

    public string TargetDirectory { get; }

    public IReadOnlyList<string> SourceFiles { get; }

    private static string NormalizeDirectory(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim();
    }

    private static IList<string> BuildSourceList(IEnumerable<string> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var normalizedSources = new List<string>();
        foreach (var source in sources)
        {
            normalizedSources.Add(NormalizeSource(source));
        }

        if (normalizedSources.Count == 0)
        {
            throw new ArgumentException("At least one source file must be specified.", nameof(sources));
        }

        return normalizedSources;
    }

    private static string NormalizeSource(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        return source.Trim();
    }
}
