using TaiwuModdingLib.Core.Plugin;

namespace RollProtagonist.Backend;

[PluginConfig("roll-protagonist", "lf2", "1.0.0")]
public class BackendExport : TaiwuRemakeHarmonyPlugin
{
    static BackendExport()
    {
        CosturaUtility.Initialize();
    }

    public override void Initialize()
    {
        RollProtagonistBuilder.ModIdStr = ModIdStr;
        HarmonyInstance.PatchAll(typeof(RollProtagonistBuilder));
    }
}