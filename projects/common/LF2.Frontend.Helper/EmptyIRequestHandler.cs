namespace LF2.Frontend.Helper;

public class EmptyIRequestHandler : IAsyncMethodRequestHandler
{
    public static EmptyIRequestHandler Default = new();

    public void ClearAsyncMethodCalls()
    {
    }

    public void RegisterAsyncMethodCall(int requestId)
    {
    }
}