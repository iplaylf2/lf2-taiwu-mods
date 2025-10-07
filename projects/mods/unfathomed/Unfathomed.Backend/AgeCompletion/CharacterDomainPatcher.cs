using GameData.Domains.Character;
using GameData.Domains.Character.Ai;
using GameData.Domains.Character.Relation;
using HarmonyLib;
using LF2.Game.Helper;
using System.Reflection.Emit;
using Transil.Attributes;
using Transil.Operations;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(CharacterDomain))]
internal static class CharacterDomainPatcher
{
    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static (sbyte, sbyte) FixAdoreAgeGroup
    (
        [ConsumeStackValue] Character subject,
        [ConsumeStackValue] Character @object,
        [InjectArgumentValue(0)] CharacterDomain characterDomain
    )
    {
        var subjectAgeGroup = subject.GetAgeGroup();
        var objectAgeGroup = @object.GetAgeGroup();

        if (subjectAgeGroup == AgeGroup.Adult && objectAgeGroup == AgeGroup.Adult)
        {
            return (subjectAgeGroup, objectAgeGroup);
        }

        _ = characterDomain.TryGetRelation(subject.GetId(), @object.GetId(), out var relation);

        if
        (
            !RelationType.HasRelation(relation.RelationType, RelationType.HusbandOrWife)
            && AiHelper.Relation.CanStartRelation_Adored(relation, subject.GetBehaviorType())
        )
        {
            return (AgeGroup.Adult, AgeGroup.Adult);
        }

        return (subjectAgeGroup, objectAgeGroup);
    }

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
            .ThrowIfInvalid("Anchor no matched.")
            .Advance(1)
            .InsertAndAdvance
            ([
                new(OpCodes.Ldarg_S, characterArg),
                new(OpCodes.Ldloc_S, elementObjectsLoc),
            ]);

            ILManipulator.ApplyTransformation(matcher, FixAdoreAgeGroup);

            _ = matcher
            .InsertAndAdvance
            ([
                new(OpCodes.Dup),
                new(OpCodes.Ldfld, item1Field),
                new(OpCodes.Stloc_2),
                new(OpCodes.Ldfld, item2Field),
                new(OpCodes.Stloc_S, ageGroup2Loc),
            ]);

            StructuredLogger.Info("FixAdoreAgeGroup");

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