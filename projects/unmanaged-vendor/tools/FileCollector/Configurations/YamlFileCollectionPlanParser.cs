using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace FileCollector.Configurations;

internal sealed class YamlFileCollectionPlanParser(IDeserializer deserializer)
{
    public YamlFileCollectionPlanParser()
        : this(new StaticDeserializerBuilder(new StaticContext()).IgnoreUnmatchedProperties().Build())
    {
    }

    public FileCollectionPlan Parse(TextReader reader)
    {
        try
        {
            var rawEntries = deserializer.Deserialize<List<YamlFileCollectionEntry>>(reader);
            if (rawEntries is not { Count: > 0 })
            {
                throw new FileCollectionConfigurationException("Configuration file is empty. At least one entry is required.");
            }

            var entries = rawEntries.Select(CreateEntry).ToArray();
            return new FileCollectionPlan(entries);
        }
        catch (YamlException ex)
        {
            throw new FileCollectionConfigurationException("Failed to parse YAML configuration content.", ex);
        }
        catch (InvalidDataException ex)
        {
            throw new FileCollectionConfigurationException(ex.Message, ex);
        }
    }

    public FileCollectionPlan ParseFromString(string yamlContent)
    {
        using var reader = new StringReader(yamlContent);
        return Parse(reader);
    }

    private static FileCollectionEntry CreateEntry(YamlFileCollectionEntry rawEntry)
    {
        var entry = rawEntry ?? throw new InvalidDataException("Configuration entry cannot be null.");

        var targetDirectory = NormalizeDirectory(entry.TargetDirectory);
        var sourceFiles = NormalizeSources(entry.SourceFiles);

        return new FileCollectionEntry(targetDirectory, sourceFiles);
    }

    private static string NormalizeDirectory(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new InvalidDataException($"Configuration entry is missing the {FileCollectionFields.TargetDirectory} field.");
    }

    private static IReadOnlyList<string> NormalizeSources(List<string>? values)
    {
        if (values is not { Count: > 0 })
        {
            throw new InvalidDataException($"Each configuration entry must contain at least one {FileCollectionFields.SourceFiles} value.");
        }

        var normalized = new List<string>(values.Count);
        for (var index = 0; index < values.Count; index++)
        {
            var source = values[index];
            if (string.IsNullOrWhiteSpace(source))
            {
                throw new InvalidDataException($"Configuration entry contains an empty {FileCollectionFields.SourceFiles} value at index {index}.");
            }

            normalized.Add(source.Trim());
        }

        return [.. normalized];
    }

    [SuppressMessage
    ("Performance", "CA1812: Avoid uninstantiated internal classes", Justification = "<Pending>")
    ]
    private sealed class YamlFileCollectionEntry
    {
        [YamlMember(Alias = FileCollectionFields.TargetDirectory)]
        public string? TargetDirectory { get; set; }

        [YamlMember(Alias = FileCollectionFields.SourceFiles)]
        public List<string>? SourceFiles { get; set; }
    }
}
