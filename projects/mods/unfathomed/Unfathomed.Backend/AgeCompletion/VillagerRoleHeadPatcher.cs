using GameData.Domains.Taiwu.VillagerRole;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(VillagerRoleHead))]
internal static class VillagerRoleHeadPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(VillagerRoleHead.HandleAdore))]
    private static IEnumerable<CodeInstruction> HandleAdore
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixGetAgeGroupResult(instructions);
    }
}