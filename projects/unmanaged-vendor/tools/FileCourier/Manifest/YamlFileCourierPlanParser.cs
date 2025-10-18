using System.Diagnostics.CodeAnalysis;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace FileCourier.Manifest;

internal sealed class YamlFileCourierPlanParser(IDeserializer deserializer)
{
    public YamlFileCourierPlanParser()
        : this(new StaticDeserializerBuilder(new StaticContext()).IgnoreUnmatchedProperties().Build())
    {
    }

    public FileCourierPlan Parse(TextReader reader)
    {
        try
        {
            var rawEntries = deserializer.Deserialize<List<YamlFileCourierEntry>>(reader);
            if (rawEntries is not { Count: > 0 })
            {
                throw new FileCourierConfigurationException("Configuration file is empty. At least one entry is required.");
            }

            var errors = new List<string>();
            var entries = new List<FileCourierEntry>(rawEntries.Count);

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
                ? throw new FileCourierConfigurationException(BuildErrorMessage(errors))
                : new FileCourierPlan(entries);
        }
        catch (YamlException ex)
        {
            throw new FileCourierConfigurationException("Failed to parse YAML configuration content.", ex);
        }
    }

    public FileCourierPlan ParseFromString(string yamlContent)
    {
        using var reader = new StringReader(yamlContent);
        return Parse(reader);
    }

    private static string? TryCreateEntry(YamlFileCourierEntry entry, int index, out FileCourierEntry? result)
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
            result = new FileCourierEntry(targetDirectory, sourceFiles);
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
            return $"{context}: Missing {FileCourierFields.TargetDirectory} value.";
        }

        normalized = value.Trim();
        return null;
    }

    private static string? TryNormalizeSourceFiles(List<string>? values, string context, out IReadOnlyList<string>? normalized)
    {
        if (values is not { Count: > 0 } typedValues)
        {
            normalized = null;
            return $"{context}: At least one {FileCourierFields.SourceFiles} value is required.";
        }

        var errors = new List<string>();
        var entries = new List<string>(typedValues.Count);

        for (var index = 0; index < typedValues.Count; index++)
        {
            var source = typedValues[index];
            if (string.IsNullOrWhiteSpace(source))
            {
                errors.Add($"{context}: {FileCourierFields.SourceFiles} value at index {index} is empty.");
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
    private sealed class YamlFileCourierEntry
    {
        [YamlMember(Alias = FileCourierFields.TargetDirectory)]
        public string? TargetDirectory { get; set; }

        [YamlMember(Alias = FileCourierFields.SourceFiles)]
        public List<string>? SourceFiles { get; set; }
    }
}
