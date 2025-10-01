using GameData.Domains.LegendaryBook;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(LegendaryBookDomain))]
internal static class LegendaryBookDomainPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(LegendaryBookDomain.UpdateLegendaryBookOwnersStatuses))]
    private static IEnumerable<CodeInstruction> UpdateLegendaryBookOwnersStatuses
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return EnableChildHelper.ByHandleGetAgeGroup(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(LegendaryBookDomain.SelectHarmActionTarget))]
    private static IEnumerable<CodeInstruction> SelectHarmActionTarget
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return EnableChildHelper.ByHandleGetAgeGroup(instructions);
    }
}