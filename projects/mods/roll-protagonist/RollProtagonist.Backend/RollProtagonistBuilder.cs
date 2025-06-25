using System.Reflection;
using GameData.Common;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Utilities;
using HarmonyLib;
using LF2.Cecil.Helper;
using MonoMod.Cil;

namespace RollProtagonist.Backend;

[HarmonyPatch(typeof(CharacterDomain), nameof(CharacterDomain.CreateProtagonist))]
internal static class RollProtagonistBuilder
{
    [HarmonyILManipulator]
    private static void SplitMethod(MethodBase origin)
    {
        AdaptableLog.Info("SplitMethod started");

        var roll = MethodSegmenter.CreateLeftSegment(new LeftConfig(origin));

        AdaptableLog.Info("roll generated");

        var confirm = MethodSegmenter.CreateRightSegment(new RightConfig(origin));

        AdaptableLog.Info("confirm generated");
    }

    private class LeftConfig(MethodBase origin) :
       MethodSegmenter.LeftConfig<RollDelegate>((MethodInfo)origin, [])
    {
        protected override void InjectSplitPoint(ILCursor ilCursor)
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

    private class RightConfig(MethodBase origin) :
        MethodSegmenter.RightConfig<ConfirmDelegate>((MethodInfo)origin)
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

    private delegate Tuple<object[], bool, object[]> RollDelegate(
        CharacterDomain instance,
        DataContext context,
        ProtagonistCreationInfo info
    );
    private delegate int ConfirmDelegate(
        CharacterDomain instance,
        DataContext context,
        ProtagonistCreationInfo info,
        object[] variables
    );
}