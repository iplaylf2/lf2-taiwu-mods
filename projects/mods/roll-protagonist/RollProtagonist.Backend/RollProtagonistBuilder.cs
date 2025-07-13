using System.Reflection;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Domains.Character.Display;
using GameData.Domains.Mod;
using GameData.Utilities;
using HarmonyLib;
using LF2.Cecil.Helper;
using LF2.Game.Helper;
using MonoMod.Cil;
using RollProtagonist.Common;

namespace RollProtagonist.Backend;

[HarmonyPatch(typeof(CharacterDomain), nameof(CharacterDomain.CreateProtagonist))]
internal static class RollProtagonistBuilder
{
    public static string? ModIdStr { get; set; }

    [HarmonyILManipulator]
    private static void BuildCreationFlow(MethodBase origin)
    {
        AdaptableLog.Info("CreateFlow started");

        var roll = MethodSegmenter.CreateLeftSegment(new RollOperationConfig(origin));

        AdaptableLog.Info($"{nameof(roll)} generated");

        var commit = MethodSegmenter.CreateRightSegment(new CommitOperationConfig(origin));

        AdaptableLog.Info($"{nameof(commit)} generated");

        creationFlow = new CreateProtagonistFlow(roll, commit);

        DomainManager.Mod.AddModMethod(
            ModIdStr,
            nameof(ModConstants.Method.ExecuteInitial),
            (context, data) =>
            {
                data.Get(
                    ModConstants.Method.ExecuteInitial.Parameters.creationInfo,
                    out string infoString
                );
                var info = StringSerializer.Deserialize<ProtagonistCreationInfo>(infoString);

                creationFlow.ExecuteInitial(context, info!);
            }
        );

        DomainManager.Mod.AddModMethod(
            ModIdStr,
            nameof(ModConstants.Method.ExecuteRoll),
            (context, data) =>
            {
                var character = creationFlow.ExecuteRoll();

                var serializableModData = new SerializableModData();

                serializableModData.Set(
                    ModConstants.Method.ExecuteRoll.Return.character,
                    new CharacterDisplayDataForTooltip(character)
                );

                return serializableModData;
            }
        );
    }

    [HarmonyPrefix]
    private static bool CreateProtagonist(ref int __result)
    {
        __result = creationFlow!.ExecuteCommit();

        return false;
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

    private static CreateProtagonistFlow? creationFlow;
}