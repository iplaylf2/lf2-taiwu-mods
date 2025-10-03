using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using GameData.Domains.Character;
using HarmonyLib;
using LF2.Game.Helper;
using Transil.Attributes;
using Transil.Operations;

namespace Unfathomed.Backend.AgeCompletion;

internal static class BabyAsAdultHelper
{
    public static IEnumerable<CodeInstruction> ByFixGetAgeGroupResult
    (
        IEnumerable<CodeInstruction> instructions,
        [CallerMemberName] string callerMember = ""
    )
    {
        var matcher = new CodeMatcher(instructions);

        {
            var targetMethod = AccessTools.Method
            (
                typeof(AgeGroup),
                nameof(AgeGroup.GetAgeGroup)
            );

            _ = ApplyFixGetAgeGroupResult(matcher, targetMethod, callerMember);
        }

        {
            var targetMethod = AccessTools.Method
            (
                typeof(Character),
                nameof(Character.GetAgeGroup)
            );

            _ = ApplyFixGetAgeGroupResult(matcher, targetMethod, callerMember);
        }

        return matcher.InstructionEnumeration();
    }

    private static CodeMatcher ApplyFixGetAgeGroupResult
    (
        CodeMatcher matcher,
        MethodInfo targetMethod,
        string callerMember
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

                StructuredLogger.Info("FixGetAgeGroupResult", new { targetMethod }, callerMember);
            }
        );
    }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static sbyte FixGetAgeGroupResult
    (
        [ConsumeStackValue] sbyte original
    )
    {
        return AgeGroup.Adult;
    }
}