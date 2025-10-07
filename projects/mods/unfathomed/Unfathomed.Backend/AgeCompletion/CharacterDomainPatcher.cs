using System.Reflection.Emit;
using GameData.Domains.Character;
using HarmonyLib;
using LF2.Game.Helper;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(CharacterDomain))]
internal static class CharacterDomainPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CharacterDomain.AssassinationByJieqing))]
    private static IEnumerable<CodeInstruction> AssassinationByJieqing
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixGetAgeGroupResult(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CharacterDomain.CreateSkeletonCharacter))]
    private static IEnumerable<CodeInstruction> CreateSkeletonCharacter
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixGetAgeGroupResult(instructions);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage
    ("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")
    ]
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CharacterDomain.GetPotentialRelatedCharactersInSet))]
    private static IEnumerable<CodeInstruction> GetPotentialRelatedCharactersInSet
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        try
        {
            var matcher = new CodeMatcher(instructions);

            const byte characterArg = 4;
            const byte elementObjectsLoc = 11;
            // const byte ageGroup1Loc = 2;
            const byte ageGroup2Loc = 14;

            var ageGroupTupleType = typeof(Tuple<sbyte, sbyte>);
            var item1Field = AccessTools.Field
            (
                ageGroupTupleType,
                nameof(Tuple<sbyte, sbyte>.Item1)
            );
            var item2Field = AccessTools.Field
            (
                ageGroupTupleType,
                nameof(Tuple<sbyte, sbyte>.Item2)
            );

            _ = matcher
            .Start()
            .MatchForward
            (
                false,
                new CodeMatch(OpCodes.Stloc_S, ageGroup2Loc)
            )
            .Advance(1)
            .InsertAndAdvance
            ([
                new(OpCodes.Ldarg_S, characterArg),
                new(OpCodes.Ldloc_S, elementObjectsLoc),
                new(OpCodes.Call, "todo"),
                new(OpCodes.Dup),
                new(OpCodes.Ldfld, item1Field),
                new(OpCodes.Stloc_2),
                new(OpCodes.Ldfld, item2Field),
                new(OpCodes.Stloc_S, ageGroup2Loc),
            ]);

            return matcher.InstructionEnumeration();
        }
        catch (Exception e)
        {
            StructuredLogger.Info("Target IL has changed.", new { e.Message });

            return instructions;
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CharacterDomain.RemoveAbnormalSkeletonCharacters))]
    private static IEnumerable<CodeInstruction> RemoveAbnormalSkeletonCharacters
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixGetAgeGroupResult(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CharacterDomain.SimulateCharacterCombatResult))]
    private static IEnumerable<CodeInstruction> SimulateCharacterCombatResult
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixGetAgeGroupResult(instructions);
    }
}