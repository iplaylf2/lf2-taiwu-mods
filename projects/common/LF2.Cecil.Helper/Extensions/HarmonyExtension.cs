using HarmonyLib;

namespace LF2.Cecil.Helper.Extensions;

public static class HarmonyExtension
{
    public static void PatchMultiple(this Harmony harmony, IEnumerable<Type> patchers)
    {
        foreach (var patcher in patchers)
        {
            harmony.PatchAll(patcher);
        }
    }
}
