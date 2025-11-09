using LF2.Cecil.Helper.Extensions;
using LF2.Kit.Service;
using RollProtagonist.Common;
using RollProtagonist.Frontend.NewGamePlus.Patches;
using TaiwuModdingLib.Core.Plugin;

namespace RollProtagonist.Frontend;

[PluginConfig("roll-protagonist", "lf2", "1.0.0")]
public class ModEntry : TaiwuRemakeHarmonyPlugin
{
    public override void Initialize()
    {
        _ = ModServiceRegistry.Add(new ModConfig(ModIdStr));

        HarmonyInstance.PatchMultiple
        (
            typeof(MouseTipBasePatch),
            typeof(UI_NewGamePatch)
        );
    }

    public override void Dispose()
    {
        try
        {
            ModServiceRegistry.Clear();
        }
        finally
        {
            base.Dispose();
        }
    }
}
