using HarmonyLib;
using UnityEngine;
using GameData.Utilities;
using System.Collections;
using System.Runtime.CompilerServices;

namespace RollProtagonist.Frontend;

[HarmonyPatch(typeof(UI_NewGame))]
internal static class OnStartNewGamePatcher
{
    [HarmonyPatch("DoStartNewGame"), HarmonyReversePatch]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DoStartNewGameOrigin(UI_NewGame uiNewGame)
    {
        throw null!;
    }

    [HarmonyPatch("DoStartNewGame"), HarmonyPrefix]
    private static bool DoStartNewGamePrefix(UI_NewGame __instance)
    {
        __instance.StartCoroutine(DoStartNewGame(__instance));

        return false;
    }

    private static IEnumerator DoStartNewGame(UI_NewGame uiNewGame)
    {

        AdaptableLog.Info("before DoStartNewGame");

        DoStartNewGameOrigin(uiNewGame);

        AdaptableLog.Info("after DoStartNewGame");

        yield return new WaitForSeconds(3);

        AdaptableLog.Info("after WaitForSeconds");
    }
}