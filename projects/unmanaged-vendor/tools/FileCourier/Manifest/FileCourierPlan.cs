namespace FileCourier.Manifest;

internal sealed class FileCourierPlan(IReadOnlyList<FileCourierEntry> entries)
{
    public IReadOnlyList<FileCourierEntry> Entries { get; } = [.. entries];
}
