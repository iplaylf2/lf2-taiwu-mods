using GameData.Domains.Combat.Ai;
using GameData.Domains.Combat.Ai.Condition;
using HarmonyLib;

namespace TiredSL.Backend.CombatCheat;

internal static class FullCombatAI
{
    public static bool Enabled { get; set; }

    [HarmonyPrefix]
    [HarmonyPatch
    (
        typeof(AiController),
        nameof(AiController.IsCombatDifficultyLevel1),
        MethodType.Getter
    )]
    private static bool IsCombatDifficultyLevel1Patch(ref bool __result)
    {
        if (!Enabled)
        {
            return true;
        }

        __result = true;

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch
    (
        typeof(AiController),
        nameof(AiController.IsCombatDifficultyLevel2),
        MethodType.Getter
    )]
    private static bool IsCombatDifficultyLevel2Patch(ref bool __result)
    {
        if (!Enabled)
        {
            return true;
        }

        __result = true;

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch
    (
        typeof(AiConditionCombatDifficulty),
        nameof(AiConditionCombatDifficulty.Check)
    )]
    private static bool AiConditionCombatDifficultyCheckPatch(ref bool __result)
    {
        if (!Enabled)
        {
            return true;
        }

        __result = true;

        return false;
    }
}
