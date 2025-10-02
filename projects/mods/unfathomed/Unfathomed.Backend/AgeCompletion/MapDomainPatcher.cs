using GameData.Domains.Map;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(MapDomain))]
internal static class MapDomainPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(MapDomain.TriggerDisaster))]
    private static IEnumerable<CodeInstruction> TriggerDisaster
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return BabyAsAdultHelper.ByHandleGetAgeGroup(instructions);
    }
}