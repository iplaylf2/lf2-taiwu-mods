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

        var roll = MethodSegmenter.CreateLeftSegment(new RollConfig(origin));

        AdaptableLog.Info("roll generated");

        var afterRoll = MethodSegmenter.CreateRightSegment(new AfterRollConfig(origin));

        AdaptableLog.Info("afterRoll generated");
    }

    private class RollConfig(MethodBase origin) :
       MethodSegmenter.LeftConfig<RollDelegate>((MethodInfo)origin)
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

    private class AfterRollConfig(MethodBase origin) :
        MethodSegmenter.RightConfig<AfterRollDelegate>((MethodInfo)origin)
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
    private delegate int AfterRollDelegate(
        CharacterDomain instance,
        DataContext context,
        ProtagonistCreationInfo info,
        object[] variables
    );
}