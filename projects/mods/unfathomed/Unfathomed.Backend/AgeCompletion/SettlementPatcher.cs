using GameData.Domains.Organization;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(Settlement))]
internal static class SettlementPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Settlement.GetAvailableHighMember))]
    private static IEnumerable<CodeInstruction> GetAvailableHighMember
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixGetAgeGroupResult(instructions);
    }
}