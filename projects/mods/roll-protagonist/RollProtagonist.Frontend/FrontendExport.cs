using TaiwuModdingLib.Core.Plugin;

namespace RollProtagonist.Frontend;

[PluginConfig("roll-protagonist.frontend", "lf2", "1.0.0")]
public class FrontendExport : TaiwuRemakeHarmonyPlugin
{
    public override void Initialize()
    {
        HarmonyInstance.PatchAll(typeof(DoStartNewGamePatcher));
    }
}