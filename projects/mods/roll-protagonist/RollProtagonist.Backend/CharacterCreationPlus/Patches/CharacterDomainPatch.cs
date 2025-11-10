using GameData.Domains.Character;
using HarmonyLib;
using LF2.Cecil.Helper;
using LF2.Game.Helper;
using LF2.Kit.Service;
using MonoMod.Cil;
using RollProtagonist.Backend.CharacterCreationPlus.Core;
using RollProtagonist.Common;
using System.Reflection;

namespace RollProtagonist.Backend.CharacterCreationPlus.Patches;

[HarmonyPatch(typeof(CharacterDomain))]
internal static class CharacterDomainPatch
{
    [HarmonyPrepare]
    [HarmonyPatch(nameof(CharacterDomain.CreateProtagonist))]
    private static void CreateProtagonistTap(MethodBase originMethod)
    {
        StructuredLogger.Info("CreateProtagonistTap started");

        var offlineCreateProtagonist =
            AccessTools.Method(typeof(Character), nameof(Character.OfflineCreateProtagonist));

        IEnumerable<Type> RollOperationSplitPoint(ILCursor ilCursor)
        {
            _ = ilCursor.GotoNext
            (
                MoveType.After,
                x => x.MatchCallOrCallvirt(offlineCreateProtagonist),
                x => x.MatchStloc(out var _)
            );

            return [];
        }

        void CommitOperationContinuationPoint(ILCursor ilCursor)
        {
            _ = ilCursor.GotoNext
            (
                MoveType.After,
                x => x.MatchCallOrCallvirt(offlineCreateProtagonist),
                x => x.MatchStloc(out var _)
            );
        }

        var roll = MethodSegmenter.CreateLeftSegment<CreateProtagonistFlow.RollOperation>
        (
            (MethodInfo)originMethod,
            RollOperationSplitPoint
        );

        StructuredLogger.Info("method generated", new { method = nameof(roll) });

        var commit = MethodSegmenter.CreateRightSegment<CreateProtagonistFlow.CommitOperation>
        (
            (MethodInfo)originMethod,
            CommitOperationContinuationPoint
        );

        StructuredLogger.Info("method generated", new { method = nameof(commit) });

        var flow = ModServiceRegistry.Add(() => new CreateProtagonistFlow(roll, commit));

        _ = ModServiceRegistry.TryGet(out ModConfig? config);

        CreateProtagonistFlowTaskBinder.BindTaskCalls(config!.ModId, flow);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CharacterDomain.CreateProtagonist))]
    private static bool CreateProtagonistPrefix(ref int __result)
    {
        _ = ModServiceRegistry.TryGet(out CreateProtagonistFlow? flow);

        __result = flow!.ExecuteCommit();

        return false;
    }
}
