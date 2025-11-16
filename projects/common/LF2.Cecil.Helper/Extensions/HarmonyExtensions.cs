using HarmonyLib;

namespace LF2.Cecil.Helper.Extensions;

public static class HarmonyExtensions
{
    public static void PatchMultiple(this Harmony harmony, params IEnumerable<Type> patchers)
    {
        foreach (var patcher in patchers)
        {
            harmony.PatchAll(patcher);
        }
    }
}
