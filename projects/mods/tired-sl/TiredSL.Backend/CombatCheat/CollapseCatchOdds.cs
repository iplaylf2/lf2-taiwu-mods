using GameData.Domains;
using GameData.Domains.Combat;
using HarmonyLib;

namespace TiredSL.Backend.CombatCheat;

internal static class CollapseCatchOdds
{
    public static bool Enabled { get; set; }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CombatDomain), nameof(CombatDomain.CalcRopeHitOdds))]
    private static void CalcRopeHitOddsPatch(ref int __result, CombatDomain __instance)
    {
        if (!Enabled || DomainManager.Taiwu.GetTaiwuCharId() != __instance.GetSelfCharId())
        {
            return;
        }

        if (0 < __result)
        {
            __result = 100;
        }
    }
}
