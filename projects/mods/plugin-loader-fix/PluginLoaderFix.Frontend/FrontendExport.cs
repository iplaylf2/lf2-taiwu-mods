using PluginLoaderFix.Common;
using TaiwuModdingLib.Core.Plugin;

namespace PluginLoaderFix.Frontend;

[PluginConfig("plugin-loader-fix", "lf2", "1.0.0")]
public class FrontendExport : TaiwuRemakeHarmonyPlugin
{
    public override void Initialize()
    {
        HarmonyInstance.PatchAll(typeof(PluginHelperPatcher));
    }
}