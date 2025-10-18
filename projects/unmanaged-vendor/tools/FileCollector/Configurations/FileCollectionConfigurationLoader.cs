namespace FileCollector.Configurations;

internal static class FileCollectionConfigurationLoader
{
    public static FileCollectionPlan Load(string configurationPath)
    {
        using var stream = File.OpenRead(configurationPath);
        using var reader = new StreamReader(stream);
        var parser = new YamlFileCollectionPlanParser();
        return parser.Parse(reader);
    }
}
