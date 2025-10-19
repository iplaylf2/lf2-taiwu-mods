namespace FileCourier.Manifest;

internal static class ManifestLoader
{
    public static ManifestPlan Load(string manifestPath)
    {
        using var stream = File.OpenRead(manifestPath);
        using var reader = new StreamReader(stream);
        var parser = new YamlManifestPlanParser();
        return parser.Parse(reader);
    }
}
