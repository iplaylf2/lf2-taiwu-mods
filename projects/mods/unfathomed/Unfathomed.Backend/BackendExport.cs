using TaiwuModdingLib.Core.Plugin;

namespace Unfathomed.Backend;

[PluginConfig("unfathomed", "lf2", "1.0.0")]
public class BackendExport : TaiwuRemakeHarmonyPlugin, IDisposable
{
    public override void Initialize()
    {
        var patchers = new[]
        {
            typeof(AgeCompletion.AiConditionOptionUseItemWinePatcher),
            typeof(AgeCompletion.BuildingDomainPatcher),
            typeof(AgeCompletion.CharacterDomainPatcher),
            typeof(AgeCompletion.CharacterPatcher),
            typeof(AgeCompletion.CharacterSortFilterPatcher),
            typeof(AgeCompletion.CombatDomainPatcher),
            typeof(AgeCompletion.ExtraDomainPatcher),
            typeof(AgeCompletion.LegendaryBookDomainPatcher),
            typeof(AgeCompletion.MapDomainPatcher),
            typeof(AgeCompletion.OrganizationDomainPatcher),
            typeof(AgeCompletion.SectPatcher),
            typeof(AgeCompletion.SettlementPatcher)
        };

        foreach (var patcher in patchers)
        {
            HarmonyInstance.PatchAll(patcher);
        }
    }
}