using System.Reflection.Emit;
using GameData.Domains.Character;
using GameData.Utilities;
using HarmonyLib;
using Transil.Attributes;
using Transil.Operations;

namespace Unfathomed.Backend.AgeCompletion;

internal static class ChildAsAdultHelper
{
    public static IEnumerable<CodeInstruction> ByFixInstanceGetAgeGroup
    (
        IEnumerable<CodeInstruction> instructions
    )
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

    public static IEnumerable<CodeInstruction> ByFixStaticAgeGroup
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        var matcher = new CodeMatcher(instructions);

        var targetMethod = AccessTools.Method
        (
            typeof(AgeGroup),
            nameof(AgeGroup.GetAgeGroup)
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


    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static sbyte FixGetAgeGroup
    (
        [ConsumeStackValue] sbyte original
    )
    {
        return original >= AgeGroup.Child ? AgeGroup.Adult : AgeGroup.Baby;
    }
}