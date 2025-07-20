using Cysharp.Threading.Tasks;
using GameData.Utilities;
using UnityEngine;
using UnityEngine.LowLevel;

namespace LF2.Frontend.Helper;

public static class AutoInit
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void InitUniTaskLoop()
    {
        var loop = PlayerLoop.GetCurrentPlayerLoop();

        PlayerLoopHelper.Initialize(ref loop);

        AdaptableLog.Info("InitUniTaskLoop");
    }
}