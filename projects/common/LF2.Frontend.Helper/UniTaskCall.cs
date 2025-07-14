using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using GameData.Domains.Mod;
using GameData.GameDataBridge;
using GameData.Serializer;
using LF2.Game.Helper;

namespace LF2.Frontend.Helper;

public sealed class UniTaskCall
{
    public static UniTaskCall Default { get => lazyDefault.Value; }

    public UniTask<SerializableModData> CallModMethod(string modIdStr, string methodName, SerializableModData parameter)
    {
        var callId = nextCallId();
        var completionSource = new UniTaskCompletionSource<SerializableModData>();

        CallRegistry.TryAdd(callId, completionSource);
        parameter.Set(CommonModConstants.CallIdKey, callId);

        ModDomainHelper.MethodCall.CallModMethodWithParamAndRet(listenerId, modIdStr, methodName, parameter);

        return completionSource.Task;
    }

    public static Func<int> Create()
    {
        int currentId = int.MinValue;

        return () => Interlocked.Increment(ref currentId);
    }

    private static readonly Lazy<UniTaskCall> lazyDefault = new(() => new UniTaskCall());

    private UniTaskCall()
    {
        listenerId = GameDataBridge.RegisterListener(NotificationHandler);
    }

    private void NotificationHandler(List<NotificationWrapper> notifications)
    {
        foreach (var notification in notifications)
        {
            var offset = notification.Notification.ValueOffset;
            var modData = new SerializableModData();

            Serializer.Deserialize(notification.DataPool, offset, ref modData);

            modData.Get(CommonModConstants.CallIdKey, out int callId);

            CallRegistry.TryGetValue(callId, out var completionSource);

            completionSource.TrySetResult(modData);
        }
    }

    private readonly int listenerId;

    private readonly Func<int> nextCallId = Create();

    private readonly ConcurrentDictionary<
        int,
        UniTaskCompletionSource<SerializableModData>
    > CallRegistry = new();
}