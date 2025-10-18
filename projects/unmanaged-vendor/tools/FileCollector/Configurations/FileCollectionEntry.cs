using System.Collections.ObjectModel;

namespace FileCollector.Configurations;

internal sealed class FileCollectionEntry(string targetDirectory, IEnumerable<string> sourceFiles)
{
    public string TargetDirectory { get; } = NormalizeDirectory(targetDirectory);

    public IReadOnlyList<string> SourceFiles { get; } = new ReadOnlyCollection<string>(BuildSourceList(sourceFiles));

    private static string NormalizeDirectory(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim();
    }

    private static List<string> BuildSourceList(IEnumerable<string> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);

        var normalizedSources = new List<string>();
        foreach (var source in sources)
        {
            normalizedSources.Add(NormalizeSource(source));
        }

        return normalizedSources.Count == 0
            ? throw new ArgumentException("At least one source file must be specified.", nameof(sources))
            : normalizedSources;
    }

    private static string NormalizeSource(string source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        return source.Trim();
    }
}
