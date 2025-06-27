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
    public static string? ModIdStr { get; set; }

    [HarmonyILManipulator]
    private static void RefactorDoStartNewGame(MethodBase origin)
    {
        AdaptableLog.Info("RefactorDoStartNewGame started");

        var beforeRoll = MethodSegmenter.CreateLeftSegment(new BeforeRollConfig(origin));

        AdaptableLog.Info($"{nameof(beforeRoll)} generated");

        var afterRoll = MethodSegmenter.CreateRightSegment(new AfterRollConfig(origin));

        AdaptableLog.Info($"{nameof(afterRoll)}  generated");

        IEnumerator DoStartNewGame(UI_NewGame uiNewGame)
        {
            AdaptableLog.Info("DoStartNewGame");

            var (stackValues, isRoll, variables) = beforeRoll(uiNewGame);

            if (!isRoll)
            {
                throw new InvalidOperationException("New game initialization failed.");
            }

            AdaptableLog.Info("Before roll completed successfully");

            yield return null;

            CharacterDomainHelper.MethodCall.CreateProtagonist(
                (int)stackValues[0],
                (ProtagonistCreationInfo)stackValues[1]
            );

            yield return null;

            afterRoll(uiNewGame, variables);

            AdaptableLog.Info("After roll completed successfully");
        }

        doStartNewGame = DoStartNewGame;
    }

    [HarmonyPrefix]
    private static bool DoStartNewGamePrefix(UI_NewGame __instance)
    {
        __instance.StartCoroutine(doStartNewGame!(__instance));

        return false;
    }

    private class BeforeRollConfig(MethodBase origin) :
        MethodSegmenter.LeftConfig<
            Func<UI_NewGame, Tuple<object[], bool, object[]>>
        >((MethodInfo)origin)
    {
        protected override IEnumerable<Type> InjectSplitPoint(ILCursor ilCursor)
        {
            var createProtagonist = CharacterDomainHelper.MethodCall.CreateProtagonist;

            ilCursor.GotoNext((x) => x.MatchCallOrCallvirt(createProtagonist.GetMethodInfo()));
            ilCursor.Remove();

            return [typeof(int), typeof(ProtagonistCreationInfo)];
        }
    }

    private class AfterRollConfig(MethodBase origin) :
        MethodSegmenter.RightConfig<
            Action<UI_NewGame, object[]>
        >((MethodInfo)origin)
    {
        protected override void InjectContinuationPoint(ILCursor ilCursor)
        {
            var createProtagonist = CharacterDomainHelper.MethodCall.CreateProtagonist;

            ilCursor.GotoNext((x) => x.MatchCallOrCallvirt(createProtagonist.GetMethodInfo()));
            ilCursor.Index++;
        }
    }

    private void ExecuteInitial(ProtagonistCreationInfo creationInfo)
    {

    }

    private Character ExecuteRoll()
    {

    }

    private static Func<UI_NewGame, IEnumerator>? doStartNewGame;
}

