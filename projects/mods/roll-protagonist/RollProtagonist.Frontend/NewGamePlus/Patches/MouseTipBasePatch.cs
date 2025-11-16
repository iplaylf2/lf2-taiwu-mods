using HarmonyLib;
using LF2.Frontend.Helper;

namespace RollProtagonist.Frontend.NewGamePlus.Patches;

[HarmonyPatch(typeof(MouseTipBase))]
internal static class MouseTipBasePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MouseTipBase.LateUpdate))]
    public static bool LateUpdatePrefix(MouseTipBase __instance)
    {
        return
        __instance is not MouseTipCharacterComplete
        || __instance.GetComponent<ModResourceFactory.ModdedUIBehavior>() is null;
    }
}
