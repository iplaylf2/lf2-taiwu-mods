using RollProtagonist.Frontend.NewGamePlus.Patching;
using TaiwuModdingLib.Core.Plugin;

namespace RollProtagonist.Frontend;

[PluginConfig("roll-protagonist", "lf2", "1.0.0")]
public class ModEntry : TaiwuRemakeHarmonyPlugin
{
    public override void Initialize()
    {
        HarmonyInstance.PatchAll(typeof(MouseTipCharacterCompletePatcher));

        DoStartNewGamePatcher.ModIdStr = ModIdStr;
        HarmonyInstance.PatchAll(typeof(DoStartNewGamePatcher));
    }
}
