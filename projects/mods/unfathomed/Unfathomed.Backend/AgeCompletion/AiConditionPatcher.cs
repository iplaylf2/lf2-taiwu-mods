using GameData.Domains.Combat.Ai.Condition;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

internal static class AiConditionPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch
    (
        typeof(AiConditionOptionUseItemWine),
        nameof(AiConditionOptionUseItemWine.ExtraCheck)
    )]
    private static IEnumerable<CodeInstruction> AiConditionOptionUseItemWine_ExtraCheck
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByHandleGetAgeGroup(instructions);
    }
}