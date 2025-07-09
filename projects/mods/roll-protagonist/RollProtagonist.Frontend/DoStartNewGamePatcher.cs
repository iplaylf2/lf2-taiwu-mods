using HarmonyLib;
using GameData.Utilities;
using MonoMod.Cil;
using System.Reflection;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using LF2.Cecil.Helper;
using GameData.Domains.Character.Display;
using LF2.Frontend.Helper;
using UnityEngine;
using GameData.Domains.Mod;
using RollProtagonist.Common;
using LF2.Kit;
using Cysharp.Threading.Tasks;
using GameData.Serializer;
using FrameWork;

namespace RollProtagonist.Frontend;

[HarmonyPatch(typeof(UI_NewGame), "DoStartNewGame")]
internal static class DoStartNewGamePatcher
{
    public static string? ModIdStr { get; set; }

    [HarmonyILManipulator]
    private static void RefactorDoStartNewGame(MethodBase origin)
    {
        ModResourceFactory.CreateModCopy(
            UIElement.MouseTipCharacterComplete,
            (modInstance) =>
            {
                displayObject = modInstance;
            }
        );

        AdaptableLog.Info("RefactorDoStartNewGame started");

        var beforeRoll = MethodSegmenter.CreateLeftSegment(new BeforeRollConfig(origin));

        AdaptableLog.Info($"{nameof(beforeRoll)} generated");

        var afterRoll = MethodSegmenter.CreateRightSegment(new AfterRollConfig(origin));

        AdaptableLog.Info($"{nameof(afterRoll)}  generated");

        async UniTask DoStartNewGame(UI_NewGame uiNewGame)
        {
            AdaptableLog.Info("DoStartNewGame");

            var (stackValues, isRoll, variables) = beforeRoll(uiNewGame);

            if (!isRoll)
            {
                throw new InvalidOperationException("New game initialization failed.");
            }

            AdaptableLog.Info("Before roll completed successfully");

            var creationInfo = (ProtagonistCreationInfo)stackValues[1];

            ExecuteInitial(creationInfo);

            var character = await ExecuteRoll();

            var viewArg = new ArgumentBox();

            viewArg.Set("Data", character);

            var view= displayObject!.GetComponent<MouseTipCharacterComplete>();
            view.OnInit(viewArg);

            await Task.Delay(TimeSpan.FromSeconds(10));

            CharacterDomainHelper.MethodCall.CreateProtagonist(
                (int)stackValues[0],
                (ProtagonistCreationInfo)stackValues[1]
            );

            afterRoll(uiNewGame, variables);

            AdaptableLog.Info("After roll completed successfully");
        }

        doStartNewGame = DoStartNewGame;
    }

    [HarmonyPrefix]
    private static bool DoStartNewGamePrefix(UI_NewGame __instance)
    {
        doStartNewGame!(__instance).Forget();

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

    private static void ExecuteInitial(ProtagonistCreationInfo creationInfo)
    {
        var data = new SerializableModData();
        data.Set(
            ModConstants.Method.ExecuteInitial.Parameters.creationInfo,
            StringSerializer.SerializeToString(creationInfo)
        );

        ModDomainHelper.MethodCall.CallModMethodWithParam(
            ModIdStr!,
            nameof(ModConstants.Method.ExecuteInitial),
            data
        );
    }

    private static UniTask<CharacterDisplayDataForTooltip> ExecuteRoll()
    {
        var source = new UniTaskCompletionSource<CharacterDisplayDataForTooltip>();

        ModDomainHelper.AsyncMethodCall.CallModMethodWithRet(
            EmptyIRequestHandler.Default,
            ModIdStr!,
            nameof(ModConstants.Method.ExecuteRoll),
            (offset, pool) =>
            {
                try
                {
                    var data = new SerializableModData();

                    SerializerHolder<SerializableModData>.Deserialize(pool, offset, ref data);

                    data.Get(
                        ModConstants.Method.ExecuteRoll.Return.character,
                        out CharacterDisplayDataForTooltip character
                    );

                    source.TrySetResult(character);
                }
                catch (Exception e)
                {
                    source.TrySetException(e);
                }
            }
        );

        return source.Task;
    }

    private static GameObject? displayObject;
    private static Func<UI_NewGame, UniTask>? doStartNewGame;
}

