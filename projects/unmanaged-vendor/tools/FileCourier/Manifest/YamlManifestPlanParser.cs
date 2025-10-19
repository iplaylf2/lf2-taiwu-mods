using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace FileCourier.Manifest;

internal sealed class YamlManifestPlanParser(IDeserializer deserializer)
{
    public YamlManifestPlanParser()
        : this(new StaticDeserializerBuilder(new StaticContext()).IgnoreUnmatchedProperties().Build())
    {
    }

    public ManifestPlan Parse(TextReader reader)
    {
        try
        {
            var rawEntries = deserializer.Deserialize<List<YamlManifestEntry>>(reader);
            if (rawEntries is not { Count: > 0 })
            {
                throw new ManifestException("Manifest file is empty. At least one entry is required.");
            }

            var errors = new List<string>();
            var entries = new List<ManifestEntry>(rawEntries.Count);

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
                ? throw new ManifestException(BuildErrorMessage(errors))
                : new ManifestPlan(entries);
        }
        catch (YamlException ex)
        {
            throw new ManifestException("Failed to parse YAML manifest content.", ex);
        }
    }

    public ManifestPlan ParseFromString(string yamlContent)
    {
        using var reader = new StringReader(yamlContent);
        return Parse(reader);
    }

    private static string? TryCreateEntry(YamlManifestEntry entry, int index, out ManifestEntry? result)
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
            result = new ManifestEntry(targetDirectory, sourceFiles);
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
            return $"{context}: Missing {ManifestFields.TargetDirectory} value.";
        }

        normalized = value.Trim();
        return null;
    }

    private static string? TryNormalizeSourceFiles(IReadOnlyList<string>? values, string context, out IReadOnlyList<string>? normalized)
    {
        if (values is not { Count: > 0 } typedValues)
        {
            normalized = null;
            return $"{context}: At least one {ManifestFields.SourceFiles} value is required.";
        }

        var errors = new List<string>();
        var entries = new List<string>(typedValues.Count);

        for (var index = 0; index < typedValues.Count; index++)
        {
            var source = typedValues[index];
            if (string.IsNullOrWhiteSpace(source))
            {
                errors.Add($"{context}: {ManifestFields.SourceFiles} value at index {index} is empty.");
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
        _ = builder.AppendLine("Manifest contains invalid entries:");

        foreach (var error in errors)
        {
            _ = builder.Append("  - ").AppendLine(error);
        }

        return builder.ToString().TrimEnd();
    }
}
