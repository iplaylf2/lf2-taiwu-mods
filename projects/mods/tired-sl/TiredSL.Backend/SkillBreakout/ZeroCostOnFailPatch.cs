using System.Reflection.Emit;
using GameData.Domains.Taiwu;
using GameData.Utilities;
using HarmonyLib;
using Transil.Attributes;
using Transil.Operations;

namespace TiredSL.Backend.SkillBreakout;

[HarmonyPatch(typeof(SkillBreakPlate), nameof(SkillBreakPlate.SelectBreak))]
public static class NoCostOnFailMove
{
    public static bool Enabled { get; set; }

    [ILHijackHandler(HijackStrategy.ReplaceOriginal)]
    private static byte HandleCalcCostStep(
        [ConsumeStackValue] SkillBreakPlate plate,
        [ConsumeStackValue] SkillBreakPlateIndex index
    )
    {
        return !Enabled || plate.Current == index ? plate.CalcCostStep(index) : (byte)0;
    }
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        var targetMethod = AccessTools.Method(
            typeof(SkillBreakPlate),
            nameof(SkillBreakPlate.CalcCostStep)
        );

        _ = matcher
        .MatchForward(
            false,
            new CodeMatch(OpCodes.Call, targetMethod)
        )
        .Repeat(
            (matcher) =>
            {
                ILManipulator.ApplyTransformation(matcher, HandleCalcCostStep);

                _ = matcher.Advance(1);

                AdaptableLog.Info($"handle {targetMethod}");
            }
        );

        return matcher.InstructionEnumeration();
    }
}
