using GameData.Domains.Combat;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(CombatDomain))]
internal static class CombatDomainPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(CombatDomain.NeedShowMercy))]
    private static IEnumerable<CodeInstruction> NeedShowMercy
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByHandleGetAgeGroup(instructions);
    }
}