using System.Reflection.Emit;
using GameData.Domains;
using GameData.Domains.Building;
using GameData.Domains.Character;
using HarmonyLib;
using LF2.Game.Helper;
using Transil.Attributes;
using Transil.Operations;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(BuildingDomain))]
internal static class BuildingDomainPatcher
{
    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static Predicate<int> FixSwapSoulCeremonyRemoveWhereArg
    (
        [ConsumeStackValue] Predicate<int> _
    )
    {
        var taiwuCharId = DomainManager.Taiwu.GetTaiwuCharId();

        return (id) => id == taiwuCharId
        || !DomainManager.Character.TryGetElement_Objects(id, out var character)
        || character.GetAgeGroup() == AgeGroup.Baby;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(BuildingDomain.GetSwapSoulCeremonyBodyCharIdList))]
    private static IEnumerable<CodeInstruction> GetSwapSoulCeremonyBodyCharIdList
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        var matcher = new CodeMatcher(instructions);

        var targetMethod = AccessTools.Method
        (
            typeof(HashSet<int>),
            nameof(HashSet<int>.RemoveWhere)
        );

        _ = matcher
        .MatchForward(
            false,
            new CodeMatch(OpCodes.Call, targetMethod)
        )
        .Repeat(
            (matcher) =>
            {
                ILManipulator.ApplyTransformation(matcher, FixSwapSoulCeremonyRemoveWhereArg);

                _ = matcher.Advance(1);

                StructuredLogger.Info("FixSwapSoulCeremonyRemoveWhereArg", new { targetMethod });
            }
        );

        return matcher.InstructionEnumeration();
    }
}