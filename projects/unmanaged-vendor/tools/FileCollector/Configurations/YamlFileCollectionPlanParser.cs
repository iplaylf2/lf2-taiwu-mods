using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace FileCollector.Configurations;

public sealed class YamlFileCollectionPlanParser
{
    private readonly IDeserializer _deserializer;

    public YamlFileCollectionPlanParser()
        : this(new DeserializerBuilder().IgnoreUnmatchedProperties().Build())
    {
    }

    public YamlFileCollectionPlanParser(IDeserializer deserializer)
    {
        _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
    }

    public FileCollectionPlan Parse(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        try
        {
            var rawEntries = _deserializer.Deserialize<List<YamlFileCollectionEntry>>(reader);
            if (rawEntries is not { Count: > 0 })
            {
                throw new FileCollectionConfigurationException("Configuration file is empty. At least one entry is required.");
            }

            List<FileCollectionEntry> entries = [.. rawEntries.Select(CreateEntry)];
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
        ArgumentException.ThrowIfNullOrWhiteSpace(yamlContent);

        using var reader = new StringReader(yamlContent);
        return Parse(reader);
    }

    private static FileCollectionEntry CreateEntry(YamlFileCollectionEntry rawEntry)
    {
        if (rawEntry.TargetDirectory is null)
        {
            throw new InvalidDataException("Configuration entry is missing the target-dir field.");
        }

        if (rawEntry.SourceFiles is null || rawEntry.SourceFiles.Count == 0)
        {
            throw new InvalidDataException("Each configuration entry must contain at least one source-files value.");
        }

        return new FileCollectionEntry(rawEntry.TargetDirectory, rawEntry.SourceFiles);
    }

    [SuppressMessage("Performance", "CA1812", Justification = "YamlDotNet creates this type via reflection.")]
    private sealed class YamlFileCollectionEntry
    {
        [YamlMember(Alias = "target-dir")]
        public string? TargetDirectory { get; set; }

        [YamlMember(Alias = "source-files")]
        public List<string>? SourceFiles { get; set; }
    }
}
