using GameData.Domains;
using GameData.Domains.Combat;
using HarmonyLib;

namespace TiredSL.Backend.Combat;

[HarmonyPatch(typeof(CombatDomain), "CalcRopeHitOdds")]
public static class CollapseCatchOdds
{
    public static bool Enabled { get; set; }

    public static void Postfix(ref int __result, CombatDomain __instance)
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
