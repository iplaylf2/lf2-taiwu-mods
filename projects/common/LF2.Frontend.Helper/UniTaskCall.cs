using Cysharp.Threading.Tasks;
using GameData.Domains.Mod;
using GameData.GameDataBridge;
using GameData.Serializer;
using LF2.Game.Helper.Communication;
using System.Collections.Concurrent;

namespace LF2.Frontend.Helper;

public sealed class UniTaskCall
{
    public static UniTaskCall Default => lazyDefault.Value;

    public async UniTask<SerializableModData> CallModMethod
    (
        string modIdStr,
        string methodName,
        SerializableModData parameter
    )
    {
        var callId = nextCallId();
        var completionSource = new UniTaskCompletionSource<SerializableModData>();

        try
        {
            _ = CallRegistry.TryAdd(callId, completionSource);
            parameter.Set(CommunicationConstant.CallIdKey, callId);

            ModDomainMethod.Call.CallModMethodWithParamAndRet(listenerId, modIdStr, methodName, parameter);

            return await completionSource.Task;
        }
        finally
        {
            _ = CallRegistry.TryRemove(callId, out _);
        }

    }

    private static Func<int> Create()
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

            _ = Serializer.Deserialize(notification.DataPool, offset, ref modData);

            _ = modData.Get(CommunicationConstant.CallIdKey, out int callId);

            _ = CallRegistry.TryGetValue(callId, out var completionSource);

            _ = completionSource.TrySetResult(modData);
        }
    }

    private readonly int listenerId;

    private readonly Func<int> nextCallId = Create();

    private readonly ConcurrentDictionary
    <
        int,
        UniTaskCompletionSource<SerializableModData>
    >
    CallRegistry = new();
}