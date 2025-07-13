using Cysharp.Threading.Tasks;
using GameData.Domains.Mod;
using GameData.GameDataBridge;

namespace LF2.Frontend.Helper;

public class UniTaskCall
{
    public static readonly Lazy<UniTaskCall> Default = new(() => new UniTaskCall());

    public UniTask<SerializableModData> CallModMethod(string modIdStr, string methodName)
    {
        var completionSource = new UniTaskCompletionSource<SerializableModData>();

        ModDomainHelper.MethodCall.CallModMethodWithRet(listenerId, modIdStr, methodName);

        return completionSource.Task;
    }

    private static void NotificationHandler(List<NotificationWrapper> notifications)
    {
        foreach (var notification in notifications)
        {
        }
    }

    private UniTaskCall()
    {
        listenerId = GameDataBridge.RegisterListener(NotificationHandler);
    }

    private readonly int listenerId;
}