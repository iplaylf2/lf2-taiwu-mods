using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Mono.Cecil;
using TaiwuModdingLib.Core.Plugin;
using Transil.Attributes;
using Transil.Operations;

namespace PluginLoaderFix.Common;

[HarmonyPatch(PluginHelperName)]
public class PluginHelperPatcher
{
    private const string PluginHelperName = "TaiwuModdingLib.Core.Plugin.PluginHelper";

    [ILHijackHandler(HijackStrategy.ReplaceOriginal)]
    private static Type? GetEntrypointType(
        [ConsumeStackValue] Assembly assembly,
        [InjectArgumentValue(0)] string directoryPath,
        [InjectArgumentValue(1)] string dllName
    )
    {
        var location = Path.Combine(directoryPath, dllName);
        var harmonyPluginBaseName = typeof(TaiwuRemakeHarmonyPlugin).FullName!;
        var pluginBaseName = typeof(TaiwuRemakePlugin).FullName!;

        using var assemblyDef = AssemblyDefinition.ReadAssembly(location);

        foreach (var typeDef in assemblyDef.MainModule.Types)
        {
            if (!typeDef.IsPublic || typeDef.BaseType == null)
            {
                continue;
            }

            var baseTypeName = typeDef.BaseType.FullName;

            if (baseTypeName == harmonyPluginBaseName || baseTypeName == pluginBaseName)
            {
                return assembly.GetType(typeDef.FullName, throwOnError: false);
            }
        }

        return null;
    }

    [HarmonyPatch("LoadPlugin")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HandleGetEntrypointType(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        var pluginHelperType = AccessTools.TypeByName(PluginHelperName);
        var getEntrypointTypeMethodInfo = AccessTools.Method(pluginHelperType, "GetEntrypointType");

        matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Call, getEntrypointTypeMethodInfo)
            )
            .Repeat(
                (matcher) =>
                {
                    ILManipulator.ApplyTransformation(matcher, GetEntrypointType);

                    matcher.Advance(1);
                }
            );

        return matcher.InstructionEnumeration();
    }
}