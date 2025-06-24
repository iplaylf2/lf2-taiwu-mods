using HarmonyLib;
using GameData.Utilities;
using System.Collections;
using MonoMod.Cil;
using System.Reflection;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using LF2.Cecil.Helper;

namespace RollProtagonist.Frontend;

[HarmonyPatch(typeof(UI_NewGame), "DoStartNewGame")]
internal static class DoStartNewGamePatcher
{
    private static IEnumerator DoStartNewGame(UI_NewGame uiNewGame)
    {
        var (stackValues, isRoll, variables) = BeforeRoll!(uiNewGame);

        if (!isRoll)
        {
            throw new InvalidOperationException("New game initialization failed.");
        }

        AdaptableLog.Info("Before roll");

        yield return null;

        CharacterDomainHelper.MethodCall.CreateProtagonist(
            (int)stackValues[0],
            (ProtagonistCreationInfo)stackValues[1]
        );

        yield return null;

        AdaptableLog.Info("After roll completed successfully");

        AfterRoll!(uiNewGame, variables);
    }

    [HarmonyILManipulator]
    private static void SplitMethod(MethodBase origin)
    {
        AdaptableLog.Info("SplitMethod started");

        BeforeRoll = MethodSegmenter.CreateLeftSegment(new LeftConfig(origin));

        AdaptableLog.Info("BeforeRoll generated");

        AfterRoll = MethodSegmenter.CreateRightSegment(new RightConfig(origin));

        AdaptableLog.Info("AfterRoll generated");
    }

    [HarmonyPrefix]
    private static bool DoStartNewGamePrefix(UI_NewGame __instance)
    {
        __instance.StartCoroutine(DoStartNewGame(__instance));

        return false;
    }

    private class LeftConfig(MethodBase origin) :
        MethodSegmenter.LeftConfig<BeforeRollDelegate>(
            (MethodInfo)origin,
            [typeof(int), typeof(ProtagonistCreationInfo)]
        )
    {
        protected override void InjectSplitPoint(ILCursor ilCursor)
        {
            var createProtagonist = CharacterDomainHelper.MethodCall.CreateProtagonist;

            ilCursor.GotoNext((x) => x.MatchCallOrCallvirt(createProtagonist.GetMethodInfo()));
            ilCursor.Remove();
        }
    }

    private class RightConfig(MethodBase origin) :
        MethodSegmenter.RightConfig<AfterRollDelegate>((MethodInfo)origin)
    {
        protected override void InjectContinuationPoint(ILCursor ilCursor)
        {
            var createProtagonist = CharacterDomainHelper.MethodCall.CreateProtagonist;

            ilCursor.GotoNext((x) => x.MatchCallOrCallvirt(createProtagonist.GetMethodInfo()));
            ilCursor.Index++;
        }
    }

    private delegate Tuple<object[], bool, object[]> BeforeRollDelegate(UI_NewGame instance);
    private delegate void AfterRollDelegate(UI_NewGame instance, object[] variables);
    private static BeforeRollDelegate? BeforeRoll;
    private static AfterRollDelegate? AfterRoll;
}

