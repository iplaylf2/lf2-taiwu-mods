using GameData.Domains.Combat.Ai.Condition;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(AiConditionOptionUseItemWine))]
internal static class AiConditionOptionUseItemWinePatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(AiConditionOptionUseItemWine.ExtraCheck))]
    private static IEnumerable<CodeInstruction> ExtraCheck
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByHandleGetAgeGroup(instructions);
    }
}