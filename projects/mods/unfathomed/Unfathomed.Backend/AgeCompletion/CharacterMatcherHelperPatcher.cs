using Character = GameData.Domains.Character.Character;
using CharacterMatcher = Config.CharacterMatcher;
using CharacterMatcherItem = Config.CharacterMatcherItem;
using GameData.Domains.Character;
using HarmonyLib;
using Transil.Attributes;
using System.Reflection.Emit;
using Transil.Operations;
using LF2.Game.Helper;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(CharacterMatcherHelper))]
internal static class CharacterMatcherHelperPatcher
{
    [ILHijackHandler(HijackStrategy.ReplaceOriginal)]
    private static bool FixAgeTypeMatch
    (
        [ConsumeStackValue] ECharacterMatcherAgeType ageType,
        [ConsumeStackValue] Character character,
        [InjectArgumentValue(0)] CharacterMatcherItem matcherItem
    )
    {
        var allowChild = new HashSet<CharacterMatcherItem>()
        {
            CharacterMatcher.DefValue.EmeiPotentialVictims
        };

        return matcherItem switch
        {
            var x when allowChild.Contains(x)
                => character.GetAgeGroup() != AgeGroup.Baby,
            _ => ageType.Match(character),
        };
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CharacterMatcherHelper.Match), typeof(CharacterMatcherItem), typeof(Character))]
    private static IEnumerable<CodeInstruction> CharacterMatcherItem_Match
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        var matcher = new CodeMatcher(instructions);

        var targetMethod = AccessTools.Method
        (
            typeof(CharacterMatcherHelper),
            nameof(CharacterMatcherHelper.Match),
            [typeof(ECharacterMatcherAgeType), typeof(Character)]
        );

        _ = matcher
        .MatchForward
        (
            false,
            new CodeMatch(OpCodes.Call, targetMethod)
        )
        .Repeat(
            (matcher) =>
            {
                ILManipulator.ApplyTransformation(matcher, FixAgeTypeMatch);

                _ = matcher.Advance(1);

                StructuredLogger.Info("FixAgeTypeMatch");
            }
        );

        return matcher.InstructionEnumeration();
    }
}