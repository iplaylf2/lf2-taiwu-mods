using HarmonyLib;

namespace LF2.Cecil.Helper.Extensions;

public static class HarmonyExtension
{
    public static void PatchArray(this Harmony harmony, Type[] patchers)
    {
        foreach (var patcher in patchers)
        {
            harmony.PatchAll(patcher);
        }
    }
}
