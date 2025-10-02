using GameData.Domains.Organization;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(Sect))]
internal static class SectPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Sect.ApplyPrisonerPunishmentOnAdvanceMonth))]
    private static IEnumerable<CodeInstruction> ApplyPrisonerPunishmentOnAdvanceMonth
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixInstanceGetAgeGroup(instructions);
    }
}