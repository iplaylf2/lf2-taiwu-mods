using System.Diagnostics.CodeAnalysis;
using LF2.Kit.Service;
using RollProtagonist.Backend.CharacterCreationPlus.Patches;
using RollProtagonist.Common;
using TaiwuModdingLib.Core.Plugin;

namespace RollProtagonist.Backend;

[PluginConfig("roll-protagonist", "lf2", "1.0.0")]
public class ModEntry : TaiwuRemakeHarmonyPlugin
{
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
    public override void Initialize()
    {
        _ = ModServiceRegistry.Add(new ModConfig(ModIdStr));

        HarmonyInstance.PatchAll(typeof(CharacterDomainPatch));
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
