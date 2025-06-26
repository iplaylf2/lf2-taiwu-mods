using System.Reflection;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Utilities;
using HarmonyLib;
using LF2.Cecil.Helper;
using MonoMod.Cil;
using RollProtagonist.Common;

namespace RollProtagonist.Backend;

[HarmonyPatch(typeof(CharacterDomain), nameof(CharacterDomain.CreateProtagonist))]
internal static class RollProtagonistBuilder
{
    public static string? ModIdStr { get; set; }

    [HarmonyILManipulator]
    private static void SplitMethod(MethodBase origin)
    {
        AdaptableLog.Info("SplitMethod started");

        var roll = MethodSegmenter.CreateLeftSegment(new RollOperationConfig(origin));

        AdaptableLog.Info($"{nameof(roll)} generated");

        var commit = MethodSegmenter.CreateRightSegment(new CommitOperationConfig(origin));

        AdaptableLog.Info($"{nameof(commit)} generated");

        var flow = new CreateProtagonistFlow(roll, commit);

        DomainManager.Mod.AddModMethod(
            ModIdStr,
            ModConstants.ExecuteInitial,
            (context, data) =>
            {
            }
        );

        DomainManager.Mod.AddModMethod(
            ModIdStr,
            ModConstants.ExecuteRoll,
            (context, data) =>
            {
            }
        );

        DomainManager.Mod.AddModMethod(
            ModIdStr,
            ModConstants.ExecuteCommit,
            (context, data) =>
            {
            }
        );
    }

    private class RollOperationConfig(MethodBase origin) :
        MethodSegmenter.LeftConfig<CreateProtagonistFlow.RollOperation>(
            (MethodInfo)origin
        )
    {
        protected override IEnumerable<Type> InjectSplitPoint(ILCursor ilCursor)
        {
            var offlineCreateProtagonist =
             AccessTools.Method(typeof(Character), nameof(Character.OfflineCreateProtagonist));

            ilCursor.GotoNext(
                x => x.MatchCallOrCallvirt(offlineCreateProtagonist),
                x => x.MatchStloc(out var _)
            );
            ilCursor.Index++;

            return [];
        }
    }

    private class CommitOperationConfig(MethodBase origin) :
        MethodSegmenter.RightConfig<CreateProtagonistFlow.CommitOperation>(
            (MethodInfo)origin
        )
    {
        protected override void InjectContinuationPoint(ILCursor ilCursor)
        {
            var offlineCreateProtagonist =
             AccessTools.Method(typeof(Character), nameof(Character.OfflineCreateProtagonist));

            ilCursor.GotoNext(
                x => x.MatchCallOrCallvirt(offlineCreateProtagonist),
                x => x.MatchStloc(out var _)
            );
            ilCursor.Index++;
        }
    }
}