using System.Diagnostics.CodeAnalysis;

namespace RollProtagonist.Common;

public sealed record ModConfig
(
    string ModId
) : IDisposable
{
    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize")]
    public void Dispose()
    {
    }
}