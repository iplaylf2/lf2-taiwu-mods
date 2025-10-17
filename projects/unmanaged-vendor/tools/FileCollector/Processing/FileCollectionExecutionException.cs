using FileCollector;

namespace FileCollector.Processing;

public sealed class FileCollectionExecutionException : FileCollectionException
{
    public FileCollectionExecutionException(string message)
        : base(message)
    {
    }

    public FileCollectionExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
