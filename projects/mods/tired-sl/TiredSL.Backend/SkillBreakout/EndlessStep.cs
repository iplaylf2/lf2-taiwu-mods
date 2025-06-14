using GameData.Domains.Taiwu;
using HarmonyLib;

namespace TiredSL.Backend.SkillBreakout;

[HarmonyPatch(typeof(SkillBreakPlate), nameof(SkillBreakPlate.SelectBreak))]
public static class EndlessStep
{
    public static bool Enabled { get; set; }

    public static void Prefix(SkillBreakPlateGrid __instance)
    {
        if (!Enabled)
        {
            return;
        }

        var _stepExtraNormal = Traverse.Create(__instance).Field("_stepExtraNormal");

        int stepExtraNormal = _stepExtraNormal.GetValue<int>();

        _stepExtraNormal.SetValue(stepExtraNormal + 1);
    }
}
