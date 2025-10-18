using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Serialization;

namespace FileCourier.Manifest;

[SuppressMessage
("Performance", "CA1852: Seal internal types", Justification = "<Pending>")
]
[SuppressMessage
("Performance", "CA1812: Avoid uninstantiated internal classes", Justification = "<Pending>")
]
[SuppressMessage
("Roslynator", "RCS1043:Remove 'partial' modifier from type with a single part", Justification = "<Pending>")
]
[YamlStaticContext]
internal partial class StaticContext : YamlDotNet.Serialization.StaticContext;
