using HarmonyLib;

namespace LF2.Game.Helper.Extensions;

internal static class HarmonyExtension
{
    public static void PatchArray(this Harmony harmony, Type[] patchers)
    {
        foreach (var patcher in patchers)
        {
            harmony.PatchAll(patcher);
        }
    }
}
