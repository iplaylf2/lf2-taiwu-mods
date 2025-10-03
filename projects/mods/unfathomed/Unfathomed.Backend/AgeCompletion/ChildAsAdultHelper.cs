using System.Reflection;
using System.Reflection.Emit;
using GameData.Domains.Character;
using GameData.Utilities;
using HarmonyLib;
using Transil.Attributes;
using Transil.Operations;

namespace Unfathomed.Backend.AgeCompletion;

internal static class ChildAsAdultHelper
{
    public static IEnumerable<CodeInstruction> ByFixGetAgeGroupResult
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

            _ = ApplyFixGetAgeGroupResult(matcher, targetMethod);
        }

        {
            var targetMethod = AccessTools.Method
            (
                typeof(Character),
                nameof(Character.GetAgeGroup)
            );

            _ = ApplyFixGetAgeGroupResult(matcher, targetMethod);
        }

        return matcher.InstructionEnumeration();
    }

    private static CodeMatcher ApplyFixGetAgeGroupResult
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

                ILManipulator.ApplyTransformation(matcher, FixGetAgeGroupResult);

                _ = matcher.Advance(1);

                AdaptableLog.Info($"handle {targetMethod}");
            }
        );
    }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static sbyte FixGetAgeGroupResult
    (
        [ConsumeStackValue] sbyte original
    )
    {
        return original >= AgeGroup.Child ? AgeGroup.Adult : AgeGroup.Baby;
    }
}