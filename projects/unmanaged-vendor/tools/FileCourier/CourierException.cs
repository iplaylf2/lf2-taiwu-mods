namespace FileCourier;

internal class CourierException : Exception
{
    public CourierException()
    {
    }

    public CourierException(string message)
        : base(message)
    {
    }

    public CourierException(string message, Exception innerException)
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
