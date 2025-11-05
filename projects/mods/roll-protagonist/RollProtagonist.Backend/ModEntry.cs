using RollProtagonist.Backend.CharacterCreationPlus.Patches;
using TaiwuModdingLib.Core.Plugin;

namespace RollProtagonist.Backend;

[PluginConfig("roll-protagonist", "lf2", "1.0.0")]
public class ModEntry : TaiwuRemakeHarmonyPlugin
{
    public override void Initialize()
    {
        CreateProtagonistPatch.ModIdStr = ModIdStr;
        HarmonyInstance.PatchAll(typeof(CreateProtagonistPatch));
    }
}
