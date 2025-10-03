using System.Reflection.Emit;
using GameData.Domains.Character;
using HarmonyLib;
using LF2.Game.Helper;
using Redzen.Random;
using Transil.Attributes;
using Transil.Operations;

namespace Unfathomed.Backend.DarkAshCompletion;

[HarmonyPatch(typeof(CharacterDomain))]
internal static class CharacterDomainPatcher
{
    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static short FixGetActualAgeResult
    (
        [ConsumeStackValue] short original
    )
    {
        return AgeGroup.GetAgeGroup(original) switch
        {
            AgeGroup.Baby => 70,
            AgeGroup.Child => 40,
            _ => original,
        };
    }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static int FixNextArg
    (
        [ConsumeStackValue] int original
    )
    {
        return Math.Max(0, original);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CharacterDomain.MarkExceedPopulations))]
    private static IEnumerable<CodeInstruction> MarkExceedPopulations
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        var matcher = new CodeMatcher(instructions);

        {
            var targetMethod = AccessTools.Method
            (
                typeof(Character),
                nameof(Character.GetActualAge)
            );

            _ = matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Call, targetMethod)
            )
            .Repeat(
                (matcher) =>
                {
                    _ = matcher.Advance(1);

                    ILManipulator.ApplyTransformation(matcher, FixGetActualAgeResult);

                    _ = matcher.Advance(1);

                    StructuredLogger.Info("FixGetActualAgeResult");
                }
            );
        }

        {
            var targetMethod = AccessTools.Method
            (
                typeof(IRandomSource),
                nameof(IRandomSource.Next)
            );

            _ = matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Call, targetMethod)
            )
            .Repeat(
                (matcher) =>
                {
                    ILManipulator.ApplyTransformation(matcher, FixNextArg);

                    _ = matcher.Advance(1);

                    StructuredLogger.Info("FixNextArg");
                }
            );
        }

        return matcher.InstructionEnumeration();
    }
}