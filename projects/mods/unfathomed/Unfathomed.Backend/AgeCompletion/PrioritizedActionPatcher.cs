using GameData.Domains.Character.Ai.PrioritizedAction;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

internal static class PrioritizedActionPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch
    (
        typeof(SectStoryEmeiToFightComradeAction),
        nameof(SectStoryEmeiToFightComradeAction.CheckValid)
    )]
    private static IEnumerable<CodeInstruction> SectStoryEmeiToFightComradeAction_CheckValid
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixGetAgeGroupResult(instructions);
    }
}