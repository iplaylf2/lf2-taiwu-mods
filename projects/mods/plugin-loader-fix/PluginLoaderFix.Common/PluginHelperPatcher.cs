using System.Reflection;
using HarmonyLib;

namespace PluginLoaderFix.Common;

[HarmonyPatch("TaiwuModdingLib.Core.Plugin.PluginHelper", "GetEntrypointType")]
public class PluginHelperPatcher
{
    public static bool Prefix(ref Type? __result, Assembly assembly)
    {
        __result = PluginHelperFix.GetEntrypointType(assembly);

        return false;
    }
}