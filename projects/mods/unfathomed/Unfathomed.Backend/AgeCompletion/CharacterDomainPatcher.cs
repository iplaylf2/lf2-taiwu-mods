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

            throw new NotImplementedException("todo: hardcode is all you need");

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