namespace FileCourier.Manifest;

internal sealed class ManifestException : CourierException
{
    public ManifestException()
    {
    }

    public ManifestException(string message)
        : base(message)
    {
    }

    public ManifestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
