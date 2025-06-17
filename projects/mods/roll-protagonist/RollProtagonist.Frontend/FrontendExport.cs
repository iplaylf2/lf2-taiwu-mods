using TaiwuModdingLib.Core.Plugin;

namespace RollProtagonist.Frontend;

[PluginConfig("roll-protagonist", "lf2", "1.0.0")]
public class FrontendExport : TaiwuRemakeHarmonyPlugin
{
    public override void Initialize()
    {
        HarmonyInstance.PatchAll(typeof(AssetBundlePatcher));
        HarmonyInstance.PatchAll(typeof(OnStartNewGame));
    }
}