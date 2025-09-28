using GameData.Domains.Taiwu;
using HarmonyLib;

namespace TiredSL.Backend.SkillBreakout;

[HarmonyPatch(typeof(SkillBreakPlate), nameof(SkillBreakPlate.RandomGridData))]
public static class BrightenUp
{
    public static bool Enabled { get; set; }

    public static void Postfix(ref SkillBreakPlateGrid __result)
    {
        if (!Enabled)
        {
            return;
        }

        __result._internalState = (sbyte)ESkillBreakGridState.Showed;
    }
}
