namespace FileCourier;

internal class FileCourierException : Exception
{
    public FileCourierException()
    {
    }

    public FileCourierException(string message)
        : base(message)
    {
    }

    public FileCourierException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
