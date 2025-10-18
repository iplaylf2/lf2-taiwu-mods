namespace FileCourier.Processing;

internal sealed class FileCourierExecutionException : FileCourierException
{
    public FileCourierExecutionException()
    {
    }

    public FileCourierExecutionException(string message)
        : base(message)
    {
    }

    public FileCourierExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
