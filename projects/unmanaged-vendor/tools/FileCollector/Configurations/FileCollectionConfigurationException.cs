using FileCollector;

namespace FileCollector.Configurations;

public sealed class FileCollectionConfigurationException : FileCollectionException
{
    public FileCollectionConfigurationException(string message)
        : base(message)
    {
    }

    public FileCollectionConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
