using HarmonyLib;
using UnityEngine;
using GameData.Utilities;
using System.Collections;

namespace RollProtagonist.Frontend;

[HarmonyPatch(typeof(UI_NewGame))]
internal static class OnStartNewGamePatcher
{
    [HarmonyPatch("DoStartNewGame"), HarmonyPrefix]
    private static bool DoStartNewGamePrefix(UI_NewGame __instance)
    {
        InDoStartGame = true;

        __instance.StartCoroutine(DoStartNewGame(__instance));

        return false;
    }


    private static IEnumerator DoStartNewGame(UI_NewGame newGameInstance)
    {

        AdaptableLog.Info("before DoStartNewGame");

        Traverse.Create(newGameInstance).Method("DoStartNewGame").GetValue();

        AdaptableLog.Info("after DoStartNewGame");

        yield return new WaitForSeconds(3);

        InDoStartGame = false;
    }

    [HarmonyPatch("OnStartNewGame"), HarmonyPrefix]
    private static bool OnStartNewGame()
    {
        return !InDoStartGame;
    }

    private static bool InDoStartGame = false;
}