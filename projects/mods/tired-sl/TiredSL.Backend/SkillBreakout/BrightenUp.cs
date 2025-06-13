using GameData.Domains.Taiwu;
using HarmonyLib;

namespace TiredSL.Backend.SkillBreakout;

[HarmonyPatch(typeof(SkillBreakPlate), "RandomGridData")]
public static class BrightenUp
{
    public static bool Enabled;

    public static void Postfix(ref SkillBreakPlateGrid __result)
    {
        if (!Enabled)
        {
            return;
        }

        Traverse
            .Create(__result)
            .Field("_internalState")
            .SetValue((sbyte)ESkillBreakGridState.Showed);
    }
}
