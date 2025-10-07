
using System.Reflection.Emit;
using GameData.Domains.Character;
using HarmonyLib;
using LF2.Game.Helper;
using Transil.Attributes;
using Transil.Operations;

namespace Unfathomed.Backend.DarkAshCompletion;

[HarmonyPatch(typeof(Character))]
internal static class CharacterPatcher
{

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static int FixBaseTimeValue
    (
        [ConsumeStackValue] int original,
        [InjectArgumentValue(0)] Character instance
    )
    {
        var age = instance.GetActualAge();

        if (AgeGroup.GetAgeGroup(age) == AgeGroup.Adult)
        {
            return original;
        }

        var leftTime =
        (GlobalConfig.AgeAdult * 12)
        - CharacterDomain.GetLivedMonths(age, instance.GetBirthMonth());

        return original + Math.Max(0, leftTime);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Character.AddDarkAsh))]
    private static IEnumerable<CodeInstruction> AddDarkAsh
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        var matcher = new CodeMatcher(instructions);

        _ = matcher
        .Start()
        .MatchForward
        (
            false,
            new CodeMatch(x => x.IsStarg(3))
        )
        .Repeat(
            (matcher) =>
            {
                ILManipulator.ApplyTransformation(matcher, FixBaseTimeValue);

                _ = matcher.Advance(1);

                StructuredLogger.Info("FixBaseTimeValue");
            }
        );

        return matcher.InstructionEnumeration();
    }
}