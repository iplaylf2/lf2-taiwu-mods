using Cysharp.Threading.Tasks;
using FrameWork;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using HarmonyLib;
using LF2.Cecil.Helper;
using LF2.Frontend.Helper;
using LF2.Kit.Extensions;
using MonoMod.Cil;
using RollProtagonist.Frontend.NewGamePlus.Core;
using System.Reflection;

namespace RollProtagonist.Frontend.NewGamePlus.Patching;

[HarmonyPatch(typeof(UI_NewGame), "DoStartNewGame")]
internal static class DoStartNewGamePatcher
{
    public static string? ModIdStr { get; set; }

    [HarmonyILManipulator]
    private static void RefactorDoStartNewGame(MethodBase origin)
    {
        Game.ClockAndLogInfo("RefactorDoStartNewGame started", false);

        static IEnumerable<Type> BeforeRollSplitPoint(ILCursor ilCursor)
        {
            var createProtagonist = CharacterDomainMethod.Call.CreateProtagonist;

            _ = ilCursor
            .GotoNext(x => x.MatchCallOrCallvirt(createProtagonist.GetMethodInfo()))
            .Remove();

            return
            [
                typeof(int),
                typeof(ProtagonistCreationInfo)
            ];
        }

        static void AfterRollContinuationPoint(ILCursor ilCursor)
        {
            var createProtagonist = CharacterDomainMethod.Call.CreateProtagonist;

            _ = ilCursor.GotoNext(x => x.MatchCallOrCallvirt(createProtagonist.GetMethodInfo()));
            ilCursor.Index++;
        }

        var beforeRoll = MethodSegmenter.CreateLeftSegment<Func<UI_NewGame, Tuple<object[], bool, object[]>>>
        (
            (MethodInfo)origin,
            BeforeRollSplitPoint
        );

        Game.ClockAndLogInfo($"{nameof(beforeRoll)} generated", false);

        var afterRoll = MethodSegmenter.CreateRightSegment<Action<UI_NewGame, object[]>>
        (
            (MethodInfo)origin,
            AfterRollContinuationPoint
        );

        Game.ClockAndLogInfo($"{nameof(afterRoll)}  generated", false);

        async UniTask DoStartNewGame(UI_NewGame uiNewGame)
        {
            Game.ClockAndLogInfo("DoStartNewGame", false);

            var modId = RequireModId();

            var (stackValues, isRoll, variables) = beforeRoll(uiNewGame);

            if (!isRoll)
            {
                throw new InvalidOperationException("New game initialization failed.");
            }

            if
            (
                stackValues.AsTuple() is not (int listenerId, ProtagonistCreationInfo creationInfo)
            )
            {
                throw new InvalidOperationException
                (
                    "Failed to deconstruct stack values from the original method."
                    + "The target method's structure has likely changed due to a game update."
                );
            }

            _ = SingletonObject
            .getInstance<BasicGameData>()
            .CustomTexts.AddRangeOnlyAdd
            (
                new()
                {
                    [0] = creationInfo.Surname,
                    [1] = creationInfo.GivenName
                }
            );

            Game.ClockAndLogInfo("Before roll completed", false);

            await NewGameModWorkflow.ExecuteInitial(modId, creationInfo);

            Game.ClockAndLogInfo("Execute Initial completed", false);

            var isRolling = true;

            while (isRolling)
            {
                var character = await NewGameModWorkflow.ExecuteRoll(modId);

                var viewArg = new ArgumentBox();
                _ = viewArg.Set("Data", character);

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

                    await UniTask.NextFrame();
                }
            }

            CharacterDisplay!.Destroy();

            CharacterDomainMethod.Call.CreateProtagonist(listenerId, creationInfo);

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

    [HarmonyPrepare]
    private static void Prepare()
    {
        CharacterDisplay = ModResourceFactory.CreateModCopy
        (
            () =>
            {
                var path = UIElement.MouseTipCharacterComplete._path;

                return new UIElement
                {
                    _path = path
                };
            }
        );
    }

    [HarmonyCleanup]
    private static void Cleanup()
    {
        CharacterDisplay?.Destroy();
    }

    private static string RequireModId()
    {
        return ModIdStr ?? throw new InvalidOperationException("ModIdStr must be initialized before rolling protagonist.");
    }

    private static UIElement? CharacterDisplay;
    private static Func<UI_NewGame, UniTask>? doStartNewGame;
}
