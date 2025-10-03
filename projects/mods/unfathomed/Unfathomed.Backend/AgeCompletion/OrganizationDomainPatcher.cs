using GameData.Domains.Organization;
using HarmonyLib;

namespace Unfathomed.Backend.AgeCompletion;

[HarmonyPatch(typeof(OrganizationDomain))]
internal static class OrganizationDomainPatcher
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(OrganizationDomain.TryAddSectMemberFeature))]
    private static IEnumerable<CodeInstruction> TryAddSectMemberFeature
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        return ChildAsAdultHelper.ByFixGetAgeGroupResult(instructions);
    }
}