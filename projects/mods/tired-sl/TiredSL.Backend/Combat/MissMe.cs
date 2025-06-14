using GameData.Domains.Combat;
using HarmonyLib;

namespace TiredSL.Backend.Combat;

public static class MissMe
{
    public static bool Enabled { get; set; }

    [HarmonyPatch(typeof(CombatDomain), nameof(CombatDomain.InAttackRange))]
    [HarmonyPostfix]
    public static void InAttackRange(ref bool __result, CombatDomain __instance, CombatCharacter character)
    {
        if (!Enabled || !__instance.IsInCombat())
        {
            return;
        }

        var defender = __instance.GetCombatCharacter(isAlly: !character.IsAlly, tryGetCoverCharacter: true);

        if (defender.IsTaiwu)
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(CombatDomain), nameof(CombatDomain.CanCastSkill))]
    [HarmonyPostfix]
    public static void CanCastSkill(ref bool __result, CombatDomain __instance, CombatCharacter character, short skillId, bool costFree, bool checkRange)
    {
        if (!Enabled || !__instance.IsInCombat())
        {
            return;
        }

        var defender = __instance.GetCombatCharacter(isAlly: !character.IsAlly, tryGetCoverCharacter: true);

        if (defender.IsTaiwu)
        {
            __result = false;
        }
    }
}
