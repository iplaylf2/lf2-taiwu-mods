using GameData.Domains.Combat.Ai;
using GameData.Domains.Combat.Ai.Condition;
using HarmonyLib;

namespace TiredSL.Backend.Combat;

public static class FullCombatAI
{
    public static bool Enabled { get; set; }

    [HarmonyPatch(typeof(AiController), nameof(AiController.IsCombatDifficultyLevel1), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool IsCombatDifficultyLevel1(ref bool __result)
    {
        if (!Enabled)
        {
            return true;
        }

        __result = true;

        return false;
    }

    [HarmonyPatch(typeof(AiController), nameof(AiController.IsCombatDifficultyLevel2), MethodType.Getter)]
    [HarmonyPrefix]
    public static bool IsCombatDifficultyLevel2(ref bool __result)
    {
        if (!Enabled)
        {
            return true;
        }


        __result = true;

        return false;
    }

    [HarmonyPatch(typeof(AiConditionCombatDifficulty), nameof(AiConditionCombatDifficulty.Check))]
    [HarmonyPrefix]
    public static bool Check(ref bool __result)
    {
        if (!Enabled)
        {
            return true;
        }

        __result = true;

        return false;
    }
}
