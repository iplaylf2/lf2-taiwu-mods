using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Serialization;

namespace FileCourier.Manifest;

/// <summary>
/// Provides the YAML serialization static context for file courier manifests.
/// </summary>
[SuppressMessage
("Performance", "CA1852: Seal internal types")
]
[SuppressMessage
("Performance", "CA1812: Avoid uninstantiated internal classes")
]
[SuppressMessage
("Maintainability", "CA1515:Consider making public types internal")
]
[SuppressMessage
("Roslynator", "RCS1043:Remove 'partial' modifier from type with a single part")
]
[YamlStaticContext]
public partial class StaticContext : YamlDotNet.Serialization.StaticContext;
