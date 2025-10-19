namespace FileCourier.Processing;

internal sealed class ExecutionException : CourierException
{
    public ExecutionException()
    {
    }

    public ExecutionException(string message)
        : base(message)
    {
    }

    public ExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
