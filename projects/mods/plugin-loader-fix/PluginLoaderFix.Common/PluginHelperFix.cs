using System.Reflection;
using Mono.Cecil;
using TaiwuModdingLib.Core.Plugin;

namespace PluginLoaderFix.Common;

public class PluginHelperFix
{
    public static Type? GetEntrypointType(Assembly assembly)
    {
        string harmonyPluginBaseName = typeof(TaiwuRemakeHarmonyPlugin).FullName!;
        string pluginBaseName = typeof(TaiwuRemakePlugin).FullName!;

        using AssemblyDefinition assemblyDef = AssemblyDefinition.ReadAssembly(assembly.Location);

        foreach (TypeDefinition typeDef in assemblyDef.MainModule.Types)
        {
            if (!typeDef.IsPublic || typeDef.BaseType == null)
            {
                continue;
            }

            string baseTypeName = typeDef.BaseType.FullName;
            if (baseTypeName == harmonyPluginBaseName || baseTypeName == pluginBaseName)
            {
                return assembly.GetType(typeDef.FullName, throwOnError: false);
            }
        }

        return null;
    }
}
