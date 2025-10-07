using System.Reflection.Emit;
using GameData.Domains.Character;
using HarmonyLib;
using LF2.Game.Helper;
using Transil.Attributes;
using Transil.Operations;

namespace Unfathomed.Backend.FertilityCompletion;

[HarmonyPatch(typeof(PregnantState))]
internal static class PregnantStatePatcher
{
    [ILHijackHandler(HijackStrategy.ReplaceOriginal)]
    private static HashSet<int> FixGetRelatedCharIds
    (
        [ConsumeStackValue] CharacterDomain instance,
        [ConsumeStackValue] int charId,
        [ConsumeStackValue] ushort _relationType,
        [InjectArgumentValue(3)] bool isRape
    )
    {
        _ = instance.TryGetElement_Objects(charId, out var character);

        if (isRape && character.GetGender() == Gender.Male)
        {
            return [];
        }

        var relations = instance.GetRelatedCharacters(charId);

        return [.. relations.BloodChildren.GetCollection().Where(instance.IsCharacterAlive)];
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PregnantState.CheckPregnant))]
    private static IEnumerable<CodeInstruction> CheckPregnant
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        var matcher = new CodeMatcher(instructions);

        var targetMethod = AccessTools.Method
        (
            typeof(CharacterDomain),
            nameof(CharacterDomain.GetRelatedCharIds)
        );

        _ = matcher
        .Start()
        .MatchForward
        (
            false,
            new CodeMatch(OpCodes.Callvirt, targetMethod)
        )
        .Repeat(
            (matcher) =>
            {
                ILManipulator.ApplyTransformation(matcher, FixGetRelatedCharIds);

                _ = matcher.Advance(1);

                StructuredLogger.Info("FixGetRelatedCharIds");
            }
        );

        return matcher.InstructionEnumeration();
    }
}