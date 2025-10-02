using GameData.Domains.TaiwuEvent.EventHelper;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(EventHelper))]
internal static class EventHelperPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EventHelper.GetXuehouSkeletonGraveId))]
    private static IEnumerable<CodeInstruction> GetXuehouSkeletonGraveId
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixGetAgeGroup(instructions);
    }
}