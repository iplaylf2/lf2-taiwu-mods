using GameData.Domains.Combat;
using HarmonyLib;

namespace TiredSL.Backend.CombatCheat;

internal static class MissMe
{
    public static bool Enabled { get; set; }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CombatDomain), nameof(CombatDomain.InAttackRange))]
    private static void InAttackRangePatch
    (
        ref bool __result,
        CombatDomain __instance,
        CombatCharacter character
    )
    {
        if (!Enabled || !__instance.IsInCombat())
        {
            return;
        }

        var defender = __instance.GetCombatCharacter(!character.IsAlly, true);

        if (defender.IsTaiwu)
        {
            __result = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CombatDomain), nameof(CombatDomain.CanCastSkill))]
    private static void CanCastSkill
    (
        ref bool __result,
        CombatDomain __instance,
        CombatCharacter character
    )
    {
        if (!Enabled || !__instance.IsInCombat())
        {
            return;
        }

        var defender = __instance.GetCombatCharacter(!character.IsAlly, true);

        if (defender.IsTaiwu)
        {
            __result = false;
        }
    }
}
