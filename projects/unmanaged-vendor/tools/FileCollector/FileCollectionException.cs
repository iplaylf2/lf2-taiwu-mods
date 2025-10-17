using System;

namespace FileCollector;

public class FileCollectionException : Exception
{
    public FileCollectionException()
    {
    }

    public FileCollectionException(string message)
        : base(message)
    {
    }

    public FileCollectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
