using System.Reflection.Emit;
using GameData.Domains.Character;
using GameData.Utilities;
using HarmonyLib;
using Transil.Attributes;
using Transil.Operations;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(Character))]
internal static class CharacterPatcher
{
    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static sbyte FixGetAgeGroup
    (
        [ConsumeStackValue] sbyte original
    )
    {
        return original >= AgeGroup.Child ? AgeGroup.Adult : AgeGroup.Baby;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Character.CalcAttraction))]
    private static IEnumerable<CodeInstruction> CalcAttraction
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return EnableChild(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Character.OfflineCalcGeneralAction_IncreaseHappiness))]
    private static IEnumerable<CodeInstruction> OfflineCalcGeneralAction_IncreaseHappiness
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return EnableChild(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Character.OfflineCalcGeneralAction_SocialStatus_TeaWine))]
    private static IEnumerable<CodeInstruction> OfflineCalcGeneralAction_SocialStatus_TeaWine
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return EnableChild(instructions);
    }

    private static IEnumerable<CodeInstruction> EnableChild(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        var targetMethod = AccessTools.Method
        (
            typeof(Character),
            nameof(Character.GetAgeGroup)
        );

        _ = matcher
        .MatchForward(
            false,
            new CodeMatch(OpCodes.Call, targetMethod)
        )
        .Repeat(
            (matcher) =>
            {
                _ = matcher.Advance(1);

                ILManipulator.ApplyTransformation(matcher, FixGetAgeGroup);

                _ = matcher.Advance(1);

                AdaptableLog.Info($"handle {targetMethod}");
            }
        );

        return matcher.InstructionEnumeration();
    }

}