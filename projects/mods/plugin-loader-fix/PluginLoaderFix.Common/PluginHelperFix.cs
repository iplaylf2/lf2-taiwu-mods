using System.Reflection;
using TaiwuModdingLib.Core.Plugin;

namespace PluginLoaderFix.Common;

public class PluginHelperFix
{
    public static Type? GetEntrypointType(Assembly assembly)
    {
        Type typeFromHandle = typeof(TaiwuRemakeHarmonyPlugin);
        Type typeFromHandle2 = typeof(TaiwuRemakePlugin);
        Type[] exportedTypes = assembly.GetExportedTypes();
        Type[] array = exportedTypes;
        foreach (Type type in array)
        {
            if (type.BaseType == typeFromHandle2 || type.BaseType == typeFromHandle)
            {
                return type;
            }
        }

        return null;
    }
}
