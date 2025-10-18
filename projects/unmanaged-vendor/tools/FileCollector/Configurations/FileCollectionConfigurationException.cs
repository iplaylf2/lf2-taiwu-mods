namespace FileCollector.Configurations;

internal sealed class FileCollectionConfigurationException : FileCollectionException
{
    public FileCollectionConfigurationException()
    {
    }

    public FileCollectionConfigurationException(string message)
        : base(message)
    {
    }

    public FileCollectionConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
