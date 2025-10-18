using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace FileCollector.Configurations;

internal sealed class YamlFileCollectionPlanParser(IDeserializer deserializer)
{
    private readonly IDeserializer _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));

    public YamlFileCollectionPlanParser()
        : this(new StaticDeserializerBuilder(new StaticContext()).IgnoreUnmatchedProperties().Build())
    {
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
        return rawEntry switch
        {
            { TargetDirectory: null } =>
                throw new InvalidDataException("Configuration entry is missing the target-dir field."),
            { SourceFiles: null } or { SourceFiles.Count: 0 } =>
                throw new InvalidDataException("Each configuration entry must contain at least one source-files value."),
            { TargetDirectory: var TargetDirectory, SourceFiles: var SourceFiles } =>
               new FileCollectionEntry(TargetDirectory, SourceFiles),
            _ => throw new NotImplementedException()
        };

    }

    [SuppressMessage
    ("Performance", "CA1812: Avoid uninstantiated internal classes", Justification = "<Pending>")
    ]
    private sealed class YamlFileCollectionEntry
    {
        [YamlMember(Alias = "target-dir")]
        public string? TargetDirectory { get; set; }

        [YamlMember(Alias = "source-files")]
        public List<string>? SourceFiles { get; set; }
    }
}
