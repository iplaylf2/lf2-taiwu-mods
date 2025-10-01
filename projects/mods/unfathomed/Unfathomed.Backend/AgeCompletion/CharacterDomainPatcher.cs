using System.Reflection.Emit;
using GameData.Domains.Character;
using GameData.Utilities;
using HarmonyLib;
using Transil.Attributes;
using Transil.Operations;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(CharacterDomain))]
internal static class CharacterDomainPatcher
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
    [HarmonyPatch(nameof(CharacterDomain.RemoveAbnormalSkeletonCharacters))]
    private static IEnumerable<CodeInstruction> RemoveAbnormalSkeletonCharacters
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return EnableChild(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CharacterDomain.SimulateCharacterCombatResult))]
    private static IEnumerable<CodeInstruction> SimulateCharacterCombatResult
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