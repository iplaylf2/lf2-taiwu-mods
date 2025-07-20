using HarmonyLib;
using MonoMod.Cil;
using System.Reflection;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using LF2.Cecil.Helper;
using GameData.Domains.Character.Display;
using LF2.Frontend.Helper;
using GameData.Domains.Mod;
using RollProtagonist.Common;
using Cysharp.Threading.Tasks;
using FrameWork;
using LF2.Kit.Extensions;
using LF2.Game.Helper;

namespace RollProtagonist.Frontend;

[HarmonyPatch(typeof(UI_NewGame), "DoStartNewGame")]
internal static class DoStartNewGamePatcher
{
    public static string? ModIdStr { get; set; }

    [HarmonyILManipulator]
    private static void RefactorDoStartNewGame(MethodBase origin)
    {
        CharacterDisplay = ModResourceFactory.CreateModCopy(() =>
        {
            var path = Traverse
                .Create(UIElement.MouseTipCharacterComplete)
                .Field("_path")
                .GetValue();

            var copy = new UIElement();

            Traverse
                .Create(copy)
                .Field("_path")
                .SetValue(path);

            return copy;
        });

        Game.ClockAndLogInfo("RefactorDoStartNewGame started", false);

        var beforeRoll = MethodSegmenter.CreateLeftSegment(new BeforeRollConfig(origin));

        Game.ClockAndLogInfo($"{nameof(beforeRoll)} generated", false);

        var afterRoll = MethodSegmenter.CreateRightSegment(new AfterRollConfig(origin));

        Game.ClockAndLogInfo($"{nameof(afterRoll)}  generated", false);

        async UniTask DoStartNewGame(UI_NewGame uiNewGame)
        {
            Game.ClockAndLogInfo("DoStartNewGame", false);

            var (stackValues, isRoll, variables) = beforeRoll(uiNewGame);

            if (!isRoll)
            {
                throw new InvalidOperationException("New game initialization failed.");
            }

            if (
                stackValues.AsTuple() is not (
                    Game game,
                    EGameState gameState,
                    ArgumentBox argBox,
                    int listenerId,
                    ProtagonistCreationInfo creationInfo
                )
            )
            {
                throw new InvalidOperationException(
                    "Failed to deconstruct stack values from the original method. The target method's structure has likely changed due to a game update."
                );
            }

            UIElement.FullScreenMask.Show();

            SingletonObject
                .getInstance<BasicGameData>()
                .CustomTexts.AddRangeOnlyAdd(
                    new()
                    {
                        [0] = creationInfo.Surname,
                        [1] = creationInfo.GivenName
                    }
                );

            Game.ClockAndLogInfo("Before roll completed", false);

            await ExecuteInitial(creationInfo);

            Game.ClockAndLogInfo("Execute Initial completed", false);

            var isRolling = true;

            while (isRolling)
            {
                var character = await ExecuteRoll();

                var viewArg = new ArgumentBox();
                viewArg.Set("Data", character);

                CharacterDisplay!.SetOnInitArgs(viewArg);
                CharacterDisplay!.Show();

                Game.ClockAndLogInfo("Execute Roll completed", false);

                while (true)
                {
                    if (CommonCommandKit.Enter.Check(CharacterDisplay))
                    {
                        isRolling = false;

                        Game.ClockAndLogInfo("enter", false);

                        break;
                    }

                    if (CommonCommandKit.Shift.Check(CharacterDisplay))
                    {
                        Game.ClockAndLogInfo("roll", false);

                        break;
                    }

                    await UniTask.Yield();
                }
            }

            CharacterDisplay!.Destroy();

            UIElement.FullScreenMask.Hide();

            game.ChangeGameState(gameState, argBox);

            CharacterDomainHelper.MethodCall.CreateProtagonist(listenerId, creationInfo);

            afterRoll(uiNewGame, variables);

            Game.ClockAndLogInfo("After roll completed", false);
        }

        doStartNewGame = DoStartNewGame;
    }

    [HarmonyPrefix]
    private static bool DoStartNewGamePrefix(UI_NewGame __instance)
    {
        doStartNewGame!(__instance).Forget();

        return false;
    }

    private sealed class BeforeRollConfig(MethodBase origin) :
        MethodSegmenter.LeftConfig<
            Func<UI_NewGame, Tuple<object[], bool, object[]>>
        >((MethodInfo)origin)
    {
        protected override IEnumerable<Type> InjectSplitPoint(ILCursor ilCursor)
        {
            var createProtagonist = CharacterDomainHelper.MethodCall.CreateProtagonist;

            ilCursor
                .GotoNext(x => x.MatchCallOrCallvirt(createProtagonist.GetMethodInfo()))
                .Remove();

            var changeGameState = Game.Instance.ChangeGameState;

            ilCursor
                .Clone()
                .GotoPrev(x => x.MatchCallOrCallvirt(changeGameState.GetMethodInfo()))
                .Remove();

            return [
                typeof(Game),
                typeof(EGameState),
                typeof(ArgumentBox),
                typeof(int),
                typeof(ProtagonistCreationInfo)
            ];
        }
    }

    private sealed class AfterRollConfig(MethodBase origin) :
        MethodSegmenter.RightConfig<
            Action<UI_NewGame, object[]>
        >((MethodInfo)origin)
    {
        protected override void InjectContinuationPoint(ILCursor ilCursor)
        {
            var createProtagonist = CharacterDomainHelper.MethodCall.CreateProtagonist;

            ilCursor.GotoNext(x => x.MatchCallOrCallvirt(createProtagonist.GetMethodInfo()));
            ilCursor.Index++;
        }
    }

    private static async UniTask ExecuteInitial(ProtagonistCreationInfo creationInfo)
    {
        var data = new SerializableModData();
        data.Set(
            ModConstants.Method.ExecuteInitial.Parameters.creationInfo,
            StringSerializer.Serialize(creationInfo)
        );

        await UniTaskCall.Default.CallModMethod(
            ModIdStr!,
            nameof(ModConstants.Method.ExecuteInitial),
            data
        );
    }

    private static async UniTask<CharacterDisplayDataForTooltip> ExecuteRoll()
    {
        var data = await UniTaskCall.Default.CallModMethod(
            ModIdStr!,
            nameof(ModConstants.Method.ExecuteRoll),
            new SerializableModData()
        );

        data.Get(
            ModConstants.Method.ExecuteRoll.ReturnValue.character,
            out CharacterDisplayDataForTooltip character
        );

        return character;
    }

    private static UIElement? CharacterDisplay;
    private static Func<UI_NewGame, UniTask>? doStartNewGame;
}
