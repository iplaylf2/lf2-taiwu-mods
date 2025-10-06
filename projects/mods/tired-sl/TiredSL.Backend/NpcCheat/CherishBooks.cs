using GameData.Domains.Character.Ai;
using GameData.Domains.Item;
using HarmonyLib;
using LF2.Game.Helper;
using System.Reflection.Emit;
using Transil.Attributes;
using Transil.Operations;

namespace TiredSL.Backend.NpcCheat;

internal static class CherishBooks
{
    public static bool Enabled { get; set; }

    [ILHijackHandler(HijackStrategy.ReplaceOriginal)]
    private static sbyte HandleGetGrade
    (
        [ConsumeStackValue] sbyte itemType,
        [ConsumeStackValue] short templateId
    )
    {
        return Enabled && itemType == ItemType.SkillBook
        ? (sbyte)0
        : ItemTemplateHelper.GetGrade(itemType, templateId);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Equipping), nameof(Equipping.CreateSkillBreakBonus))]
    private static IEnumerable<CodeInstruction> CreateSkillBreakBonusPatch
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        var matcher = new CodeMatcher(instructions);

        var targetMethod = AccessTools.Method
        (
            typeof(ItemTemplateHelper),
            nameof(ItemTemplateHelper.GetGrade)
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
                ILManipulator.ApplyTransformation(matcher, HandleGetGrade);

                _ = matcher.Advance(1);

                StructuredLogger.Info("HandleGetGrade");
            }
        );

        return matcher.InstructionEnumeration();
    }
}