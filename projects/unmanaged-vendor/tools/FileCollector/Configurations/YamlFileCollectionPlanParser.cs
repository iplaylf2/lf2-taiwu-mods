using System.Diagnostics.CodeAnalysis;
using System.Text;
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

            var errors = new List<string>();
            var entries = new List<FileCollectionEntry>(rawEntries.Count);

            for (var index = 0; index < rawEntries.Count; index++)
            {
                var error = TryCreateEntry(rawEntries[index], index, out var entry);
                if (error is not null)
                {
                    errors.Add(error);
                    continue;
                }

                entries.Add(entry!);
            }

            return errors.Count != 0
                ? throw new FileCollectionConfigurationException(BuildErrorMessage(errors))
                : new FileCollectionPlan(entries);
        }
        catch (YamlException ex)
        {
            throw new FileCollectionConfigurationException("Failed to parse YAML configuration content.", ex);
        }
    }

    public FileCollectionPlan ParseFromString(string yamlContent)
    {
        using var reader = new StringReader(yamlContent);
        return Parse(reader);
    }

    private static string? TryCreateEntry(YamlFileCollectionEntry entry, int index, out FileCollectionEntry? result)
    {
        var errors = new List<string>();
        var context = $"Entry {index}";

        var targetError = TryNormalizeTargetDirectory(entry.TargetDirectory, context, out var targetDirectory);
        if (targetError is not null)
        {
            errors.Add(targetError);
        }

        var sourcesError = TryNormalizeSourceFiles(entry.SourceFiles, context, out var sourceFiles);
        if (sourcesError is not null)
        {
            errors.Add(sourcesError);
        }

        if (errors.Count == 0 && targetDirectory is not null && sourceFiles is not null)
        {
            result = new FileCollectionEntry(targetDirectory, sourceFiles);
            return null;
        }

        result = null;
        return string.Join(Environment.NewLine, errors);
    }

    private static string? TryNormalizeTargetDirectory(string? value, string context, out string? normalized)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            normalized = null;
            return $"{context}: Missing {FileCollectionFields.TargetDirectory} value.";
        }

        normalized = value.Trim();
        return null;
    }

    private static string? TryNormalizeSourceFiles(List<string>? values, string context, out IReadOnlyList<string>? normalized)
    {
        if (values is not { Count: > 0 } typedValues)
        {
            normalized = null;
            return $"{context}: At least one {FileCollectionFields.SourceFiles} value is required.";
        }

        var errors = new List<string>();
        var entries = new List<string>(typedValues.Count);

        for (var index = 0; index < typedValues.Count; index++)
        {
            var source = typedValues[index];
            if (string.IsNullOrWhiteSpace(source))
            {
                errors.Add($"{context}: {FileCollectionFields.SourceFiles} value at index {index} is empty.");
                continue;
            }

            entries.Add(source.Trim());
        }

        if (errors is { Count: > 0 })
        {
            normalized = null;
            return string.Join(Environment.NewLine, errors);
        }

        normalized = entries;
        return null;
    }

    private static string BuildErrorMessage(IEnumerable<string> errors)
    {
        var builder = new StringBuilder();
        _ = builder.AppendLine("Configuration contains invalid entries:");

        foreach (var error in errors)
        {
            _ = builder.Append("  - ").AppendLine(error);
        }

        return builder.ToString().TrimEnd();
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
