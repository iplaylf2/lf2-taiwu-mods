using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using GameData.Domains.Character;
using HarmonyLib;
using LF2.Game.Helper;
using Transil.Attributes;
using Transil.Operations;

namespace Unfathomed.Backend.AgeCompletion;

internal static class ChildAsAdultHelper
{
    public static IEnumerable<CodeInstruction> ByFixGetAgeGroupResult
    (
        IEnumerable<CodeInstruction> instructions,
        [CallerMemberName] string callerMember = ""
    )
    {
        var matcher = new CodeMatcher(instructions);

        static Action<CodeMatcher> ApplyTransformation(string callerMember)
        {
            return (matcher) =>
            {
                _ = matcher.Advance(1);

                ILManipulator.ApplyTransformation(matcher, FixGetAgeGroupResult);

                _ = matcher.Advance(1);

                StructuredLogger.Info("FixGetAgeGroupResult", null, callerMember);
            };
        }

        {
            var targetMethod = AccessTools.Method
            (
                typeof(AgeGroup),
                nameof(AgeGroup.GetAgeGroup)
            );

            _ = matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Call, targetMethod)
            )
            .Repeat(ApplyTransformation(callerMember));
        }

        {
            var targetMethod = AccessTools.Method
            (
                typeof(Character),
                nameof(Character.GetAgeGroup)
            );

            _ = matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Call, targetMethod)
            )
            .Repeat(ApplyTransformation(callerMember));

            _ = matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Callvirt, targetMethod)
            )
            .Repeat(ApplyTransformation(callerMember));
        }

        return matcher.InstructionEnumeration();
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