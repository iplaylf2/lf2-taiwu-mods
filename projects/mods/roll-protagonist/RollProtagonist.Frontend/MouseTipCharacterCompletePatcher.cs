using HarmonyLib;
using LF2.Frontend.Helper;

namespace RollProtagonist.Frontend;

[HarmonyPatch(typeof(MouseTipCharacterComplete), "LateUpdate")]
internal static class MouseTipCharacterCompletePatcher
{
    public static bool Prefix(MouseTipCharacterComplete __instance)
    {
        if (__instance.GetComponent<ModResourceFactory.ModdedUIBehavior>() != null)
        {
            return false;
        }

        return true;
    }

}