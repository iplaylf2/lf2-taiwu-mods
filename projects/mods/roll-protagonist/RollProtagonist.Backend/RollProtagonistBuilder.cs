using System.Reflection;
using GameData.Common;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Utilities;
using HarmonyLib;
using LF2.Cecil.Helper;
using MonoMod.Cil;
using static GameData.Domains.Character.Character;

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
       MethodSegmenter.LeftConfig<RollDelegate>(
            (MethodInfo)origin,
            [typeof(ProtagonistFeatureRelatedStatus)]
        )
    {
        protected override void InjectSplitPoint(ILCursor ilCursor)
        {
            throw new NotImplementedException();
        }
    }

    private class RightConfig(MethodBase origin) :
        MethodSegmenter.RightConfig<ConfirmDelegate>((MethodInfo)origin)
    {
        protected override void InjectContinuationPoint(ILCursor ilCursor)
        {
            throw new NotImplementedException();
        }
    }

    private delegate Tuple<object[], bool, object[]> RollDelegate(
        CharacterDomain instance,
        DataContext context,
        ProtagonistCreationInfo info
    );
    private delegate void ConfirmDelegate(CharacterDomain instance, object[] variables);
}