using System.Collections;
using GameData.Utilities;
using HarmonyLib;
using UnityEngine;

namespace RollProtagonist.Frontend;

[HarmonyPatch(typeof(UI_NewGame), "DoStartNewGame")]
internal static class OnStartNewGame
{
    public static void HandleAssetBundleLoaded(Harmony harmony, Type baseType, string assetName, UnityEngine.Object asset)
    {
        if (TargetClassName != assetName
            || asset is not GameObject gameObject
            || gameObject.GetComponent<MonoBehaviour>() is not { } uiNewGame
            || uiNewGame.GetType() is not { } UINewGame
        )
        {
            return;
        }
    }

    private static readonly string TargetClassName = "UI_NewGame";

    private static bool Prefix(UI_NewGame __instance)
    {
        __instance.StartCoroutine(DoStartNewGame(__instance));

        return false;
    }

    private static IEnumerator DoStartNewGame(UI_NewGame newGameInstance)
    {

        AdaptableLog.Info("before DoStartNewGame");

        Traverse.Create(newGameInstance).Method("DoStartNewGame").GetValue();

        AdaptableLog.Info("after DoStartNewGame");

        yield return new WaitForSeconds(3);
    }
}