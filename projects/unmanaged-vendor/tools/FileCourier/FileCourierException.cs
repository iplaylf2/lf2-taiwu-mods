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

    public virtual string ToDisplayString()
    {
        return FormatExceptionMessages(this);
    }

    public override string ToString()
    {
        return ToDisplayString();
    }

    private static string FormatExceptionMessages(Exception? exception)
    {
        if (exception is null)
        {
            return string.Empty;
        }

        var message = !string.IsNullOrWhiteSpace(exception.Message)
            ? exception.Message
            : "An unexpected error occurred.";

        return exception.InnerException is { } innerException
        ? $"{message}\nCaused by: {FormatExceptionMessages(innerException)}"
        : message;
    }
}