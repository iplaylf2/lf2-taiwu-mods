namespace FileCollector.Processing;

internal sealed class FileCollectionExecutionException : FileCollectionException
{
    public FileCollectionExecutionException()
    {
    }

    public FileCollectionExecutionException(string message)
        : base(message)
    {
    }

    public FileCollectionExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
