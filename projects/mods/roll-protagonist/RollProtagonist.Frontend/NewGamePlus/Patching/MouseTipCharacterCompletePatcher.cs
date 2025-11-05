using HarmonyLib;
using LF2.Frontend.Helper;

namespace RollProtagonist.Frontend.NewGamePlus.Patching;

[HarmonyPatch(typeof(MouseTipBase), "LateUpdate")]
internal static class MouseTipCharacterCompletePatcher
{
    public static bool Prefix(MouseTipBase __instance)
    {
        return
        __instance is not MouseTipCharacterComplete
        || __instance.GetComponent<ModResourceFactory.ModdedUIBehavior>() is null;
    }
}
