using GameData.Domains.Character;
using HarmonyLib;

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
        return ChildAsAdultHelper.ByFixGetAgeGroup(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CharacterDomain.CreateSkeletonCharacter))]
    private static IEnumerable<CodeInstruction> CreateSkeletonCharacter
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixGetAgeGroup(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CharacterDomain.RemoveAbnormalSkeletonCharacters))]
    private static IEnumerable<CodeInstruction> RemoveAbnormalSkeletonCharacters
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixGetAgeGroup(instructions);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CharacterDomain.SimulateCharacterCombatResult))]
    private static IEnumerable<CodeInstruction> SimulateCharacterCombatResult
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixGetAgeGroup(instructions);
    }
}