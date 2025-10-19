namespace FileCourier.Manifest;

internal sealed class ManifestPlan(IReadOnlyList<ManifestEntry> entries)
{
    public IReadOnlyList<ManifestEntry> Entries { get; } = [.. entries];
}
