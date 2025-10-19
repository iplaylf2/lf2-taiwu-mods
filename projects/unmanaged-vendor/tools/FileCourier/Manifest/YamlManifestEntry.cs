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
public sealed class YamlManifestEntry
{
    /// <summary>
    /// Gets or sets the destination directory for the copied files.
    /// </summary>
    [YamlMember(Alias = ManifestFields.TargetDirectory)]
    public string? TargetDirectory { get; set; }

    /// <summary>
    /// Gets or sets the collection of source file paths to copy.
    /// </summary>
    [SuppressMessage
    ("Usage", "CA2227:Collection properties should be read only", Justification = "<Pending>")
    ]
    [SuppressMessage
    ("Design", "CA1002:Do not expose generic lists", Justification = "<Pending>")
    ]
    [YamlMember(Alias = ManifestFields.SourceFiles)]
    public List<string>? SourceFiles { get; set; }
}
