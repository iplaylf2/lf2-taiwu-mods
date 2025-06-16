using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace RollProtagonist.Frontend;

[HarmonyPatch("UI_NewGame", "DoStartNewGame")]
internal static class OnStartNewGame
{
    public static bool Prefix(MonoBehaviour __instance)
    {
        __instance.StartCoroutine(DoStartNewGame(__instance));

        return false;
    }

    private static IEnumerator DoStartNewGame(MonoBehaviour UI_NewGame)
    {
        Traverse.Create(UI_NewGame).Method("DoStartNewGame").GetValue();

        yield return new WaitForSeconds(3);
    }
}