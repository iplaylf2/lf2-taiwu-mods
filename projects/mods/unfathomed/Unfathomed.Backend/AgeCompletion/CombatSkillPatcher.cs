using GameData.Domains.SpecialEffect.CombatSkill.Wuxianjiao.FistAndPalm;
using GameData.Domains.SpecialEffect.CombatSkill.Xuannvpai.FistAndPalm;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

internal static class CombatSkillPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch
    (
        typeof(JiuSiLiHunShou),
        nameof(JiuSiLiHunShou.IsAffectChar)
    )]
    private static IEnumerable<CodeInstruction> JiuSiLiHunShou_IsAffectChar
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixInstanceGetAgeGroup(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(
        typeof(YaoJiYunYuShi),
        nameof(YaoJiYunYuShi.OnEnable)
    )]
    private static IEnumerable<CodeInstruction> YaoJiYunYuShi_OnEnable
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixInstanceGetAgeGroup(instructions);
    }
}