using GameData.Domains.Character;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(Character))]
internal static class CharacterPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Character.CalcAttraction))]
    private static IEnumerable<CodeInstruction> CalcAttraction
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByHandleGetAgeGroup(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Character.OfflineCalcGeneralAction_IncreaseHappiness))]
    private static IEnumerable<CodeInstruction> OfflineCalcGeneralAction_IncreaseHappiness
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByHandleGetAgeGroup(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Character.OfflineCalcGeneralAction_GainExp))]
    private static IEnumerable<CodeInstruction> OfflineCalcGeneralAction_GainExp
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByHandleGetAgeGroup(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Character.OfflineCalcGeneralAction_SocialStatus_TeaWine))]
    private static IEnumerable<CodeInstruction> OfflineCalcGeneralAction_SocialStatus_TeaWine
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByHandleGetAgeGroup(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Character.OfflineCalcGeneralAction_TeaWine))]
    private static IEnumerable<CodeInstruction> OfflineCalcGeneralAction_TeaWine
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByHandleGetAgeGroup(instructions);
    }
}