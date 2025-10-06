using GameData.Domains.Taiwu;
using HarmonyLib;

namespace TiredSL.Backend.SkillBreakoutCheat;

internal static class BrightenUp
{
    public static bool Enabled { get; set; }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SkillBreakPlate), nameof(SkillBreakPlate.RandomGridData))]
    private static void RandomGridDataPatch(ref SkillBreakPlateGrid __result)
    {
        if (!Enabled)
        {
            return;
        }

        __result._internalState = (sbyte)ESkillBreakGridState.Showed;
    }
}
