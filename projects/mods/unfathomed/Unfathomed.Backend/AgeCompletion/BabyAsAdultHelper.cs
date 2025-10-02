using System.Reflection;
using System.Reflection.Emit;
using GameData.Domains.Character;
using GameData.Utilities;
using HarmonyLib;
using Transil.Attributes;
using Transil.Operations;

namespace Unfathomed.Backend.AgeCompletion;

internal static class BabyAsAdultHelper
{
    public static IEnumerable<CodeInstruction> ByFixGetAgeGroup
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        var matcher = new CodeMatcher(instructions);

        {
            var targetMethod = AccessTools.Method
            (
                typeof(AgeGroup),
                nameof(AgeGroup.GetAgeGroup)
            );

            _ = ApplyFixGetAgeGroupFor(matcher, targetMethod);
        }

        {
            var targetMethod = AccessTools.Method
            (
                typeof(Character),
                nameof(Character.GetAgeGroup)
            );

            _ = ApplyFixGetAgeGroupFor(matcher, targetMethod);
        }

        return matcher.InstructionEnumeration();
    }

    private static CodeMatcher ApplyFixGetAgeGroupFor
    (
        CodeMatcher matcher,
        MethodInfo targetMethod
    )
    {
        return matcher
        .Start()
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
    }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static sbyte FixGetAgeGroup
    (
        [ConsumeStackValue] sbyte original
    )
    {
        return AgeGroup.Adult;
    }
}