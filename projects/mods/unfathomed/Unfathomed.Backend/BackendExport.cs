using TaiwuModdingLib.Core.Plugin;

namespace Unfathomed.Backend;

[PluginConfig("unfathomed", "lf2", "1.0.0")]
public class BackendExport : TaiwuRemakeHarmonyPlugin, IDisposable
{
    public override void Initialize()
    {
        HarmonyInstance.PatchAll(typeof(AgeCompletion.AiConditionOptionUseItemWinePatcher));
        HarmonyInstance.PatchAll(typeof(AgeCompletion.BuildingDomainPatcher));
        HarmonyInstance.PatchAll(typeof(AgeCompletion.CharacterDomainPatcher));
        HarmonyInstance.PatchAll(typeof(AgeCompletion.CharacterPatcher));
        HarmonyInstance.PatchAll(typeof(AgeCompletion.CharacterSortFilterPatcher));
        HarmonyInstance.PatchAll(typeof(AgeCompletion.CombatDomainPatcher));
        HarmonyInstance.PatchAll(typeof(AgeCompletion.ExtraDomainPatcher));
        HarmonyInstance.PatchAll(typeof(AgeCompletion.LegendaryBookDomainPatcher));
        HarmonyInstance.PatchAll(typeof(AgeCompletion.MapDomainPatcher));
    }
}