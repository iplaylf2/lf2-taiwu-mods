using GameData.Domains.Taiwu;
using HarmonyLib;
using LF2.Game.Helper;
using System.Reflection.Emit;
using Transil.Attributes;
using Transil.Operations;

namespace TiredSL.Backend.SkillBreakoutCheat;

internal static class NoCostOnFailMove
{
    public static bool Enabled { get; set; }

    [ILHijackHandler(HijackStrategy.ReplaceOriginal)]
    private static byte HandleCalcCostStep
    (
        [ConsumeStackValue] SkillBreakPlate plate,
        [ConsumeStackValue] SkillBreakPlateIndex index
    )
    {
        return !Enabled || plate.Current == index ? plate.CalcCostStep(index) : (byte)0;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(SkillBreakPlate), nameof(SkillBreakPlate.SelectBreak))]
    private static IEnumerable<CodeInstruction> SelectBreakPatch(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        var targetMethod = AccessTools.Method
        (
            typeof(SkillBreakPlate),
            nameof(SkillBreakPlate.CalcCostStep)
        );

        _ = matcher
        .MatchForward
        (
            false,
            new CodeMatch(OpCodes.Call, targetMethod)
        )
        .Repeat
        (
            (matcher) =>
            {
                ILManipulator.ApplyTransformation(matcher, HandleCalcCostStep);

                _ = matcher.Advance(1);

                StructuredLogger.Info("HandleCalcCostStep");
            }
        );

        return matcher.InstructionEnumeration();
    }
}
