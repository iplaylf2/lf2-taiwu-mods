namespace FileCourier.Manifest;

internal static class FileCourierConfigurationLoader
{
    public static FileCourierPlan Load(string configurationPath)
    {
        using var stream = File.OpenRead(configurationPath);
        using var reader = new StreamReader(stream);
        var parser = new YamlFileCourierPlanParser();
        return parser.Parse(reader);
    }
}
