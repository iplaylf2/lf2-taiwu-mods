using LF2.Cecil.Helper.Extensions;
using LF2.Game.Helper;
using TaiwuModdingLib.Core.Plugin;

namespace Unfathomed.Backend;

[PluginConfig("unfathomed", "lf2", "1.0.0")]
public class ModEntry : TaiwuRemakeHarmonyPlugin, IDisposable
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage
    ("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")
    ]
    public override void Initialize()
    {
        HarmonyInstance.PatchArray
        ([
            typeof(AgeCompletion.AiConditionPatcher),
            typeof(AgeCompletion.BuildingDomainPatcher),
            typeof(AgeCompletion.CharacterDomainPatcher),
            typeof(AgeCompletion.CharacterMatcherHelperPatcher),
            typeof(AgeCompletion.CharacterPatcher),
            typeof(AgeCompletion.CharacterSortFilterPatcher),
            typeof(AgeCompletion.CombatDomainPatcher),
            typeof(AgeCompletion.CombatSkillPatcher),
            typeof(AgeCompletion.EventHelperPatcher),
            typeof(AgeCompletion.ExtraDomainPatcher),
            typeof(AgeCompletion.LegendaryBookDomainPatcher),
            typeof(AgeCompletion.MapDomainPatcher),
            typeof(AgeCompletion.OrganizationDomainPatcher),
            typeof(AgeCompletion.PrioritizedActionPatcher),
            typeof(AgeCompletion.SectPatcher),
            typeof(AgeCompletion.SettlementPatcher),
            typeof(AgeCompletion.VillagerRoleHeadPatcher),

            typeof(DarkAshCompletion.CharacterDomainPatcher),
            typeof(DarkAshCompletion.CharacterPatcher),

            typeof(FertilityCompletion.PregnantStatePatcher),

            typeof(RandomCompletion.RedzenHelperPatcher),
        ]);

        try
        {
            HarmonyInstance.PatchAll(typeof(AgeCompletion.CharacterDomainUnsafePatcher));
        }
        catch (Exception e)
        {
            StructuredLogger.Info("CharacterDomainUnsafePatcher failed", new { e.Message });
        }
    }
}