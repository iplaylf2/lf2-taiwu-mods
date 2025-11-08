using System.Diagnostics.CodeAnalysis;

namespace RollProtagonist.Common;

public record ModConfig
(
    string ModIdStr
) : IDisposable
{
    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
    public void Dispose()
    {
    }
}