using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Serialization;

namespace FileCourier.Manifest;

/// <summary>
/// Represents a single file courier entry read from a YAML manifest.
/// </summary>
[SuppressMessage
("Maintainability", "CA1515:Consider making public types internal", Justification = "<Pending>")
]
[YamlSerializable]
public sealed class YamlFileCourierEntry
{
    /// <summary>
    /// Gets or sets the destination directory for the copied files.
    /// </summary>
    [YamlMember(Alias = FileCourierFields.TargetDirectory)]
    public string? TargetDirectory { get; set; }

    /// <summary>
    /// Gets or sets the collection of source file paths to copy.
    /// </summary>
    [YamlMember(Alias = FileCourierFields.SourceFiles)]
    public ReadOnlyCollection<string>? SourceFiles { get; set; }
}
