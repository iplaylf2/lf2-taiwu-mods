namespace FileCourier.Manifest;

internal sealed class FileCourierConfigurationException : FileCourierException
{
    public FileCourierConfigurationException()
    {
    }

    public FileCourierConfigurationException(string message)
        : base(message)
    {
    }

    public FileCourierConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
