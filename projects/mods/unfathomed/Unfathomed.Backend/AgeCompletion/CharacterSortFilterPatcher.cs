using GameData.Domains.Character.SortFilter;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(CharacterSortFilter))]
internal static class CharacterSortFilterPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CharacterSortFilter.MatchCanLinkInLifeDeathGate))]
    private static IEnumerable<CodeInstruction> MatchCanLinkInLifeDeathGate
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByHandleGetAgeGroup(instructions);
    }
}