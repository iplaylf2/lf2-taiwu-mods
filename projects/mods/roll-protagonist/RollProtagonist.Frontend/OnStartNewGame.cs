using HarmonyLib;
using UnityEngine;
using GameData.Utilities;
using System.Collections;
using MonoMod.Cil;
using System.Reflection;
using MonoMod.Utils;

namespace RollProtagonist.Frontend;

[HarmonyPatch(typeof(UI_NewGame), "DoStartNewGame")]
internal static class OnStartNewGamePatcher
{
    [HarmonyILManipulator]
    private static void SplitMethodIntoStages(MethodBase origin)
    {
        var ilContext = new ILContext(new DynamicMethodDefinition(origin).Definition);
        var ilCursor = new ILCursor(ilContext);
    }

    [HarmonyPrefix]
    private static bool DoStartNewGamePrefix(UI_NewGame __instance)
    {
        __instance.StartCoroutine(DoStartNewGame(__instance));

        return false;
    }

    private static IEnumerator DoStartNewGame(UI_NewGame uiNewGame)
    {

        AdaptableLog.Info("before DoStartNewGame");

        // DoStartNewGameOrigin(uiNewGame);

        AdaptableLog.Info("after DoStartNewGame");

        yield return new WaitForSeconds(3);

        AdaptableLog.Info("after WaitForSeconds");
    }
}