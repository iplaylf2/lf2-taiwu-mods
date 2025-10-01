using GameData.Domains.Extra;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(ExtraDomain))]
internal static class ExtraDomainPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ExtraDomain.CastTasterUltimateSkill))]
    private static IEnumerable<CodeInstruction> CastTasterUltimateSkill
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return EnableChildHelper.ByHandleGetAgeGroup(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ExtraDomain.GetThreeVitalsTargetCharDataList))]
    private static IEnumerable<CodeInstruction> GetThreeVitalsTargetCharDataList
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return EnableChildHelper.ByHandleGetAgeGroup(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ExtraDomain.ApplyFulongOutLawAdvanceMonth))]
    private static IEnumerable<CodeInstruction> ApplyFulongOutLawAdvanceMonth
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return EnableChildHelper.ByHandleGetAgeGroup(instructions);
    }
}