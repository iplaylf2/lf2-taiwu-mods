using Cysharp.Threading.Tasks;
using FrameWork;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Domains.Character.Display;
using GameData.Domains.Mod;
using HarmonyLib;
using LF2.Cecil.Helper.MethodSegmentation;
using LF2.Frontend.Helper;
using LF2.Game.Helper.Communication;
using LF2.Kit.Extensions;
using MonoMod.Cil;
using RollProtagonist.Common;
using System.Reflection;

namespace RollProtagonist.Frontend;

[HarmonyPatch(typeof(UI_NewGame), "DoStartNewGame")]
internal static class DoStartNewGamePatcher
{
    public static string? ModIdStr { get; set; }

    [HarmonyILManipulator]
    private static void RefactorDoStartNewGame(MethodBase origin)
    {
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

            await ExecuteInitial(creationInfo);

            Game.ClockAndLogInfo("Execute Initial completed", false);

            var isRolling = true;

            while (isRolling)
            {
                var character = await ExecuteRoll();

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

    private sealed class BeforeRollConfig(MethodBase origin) :
        ISplitConfig<Func<UI_NewGame, Tuple<object[], bool, object[]>>>
    {
        public MethodInfo Prototype { get; } = (MethodInfo)origin;

        public IEnumerable<Type> InjectSplitPoint(ILCursor ilCursor)
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
    }

    private sealed class AfterRollConfig(MethodBase origin) :
    IContinuationConfig<Action<UI_NewGame, object[]>>
    {
        public MethodInfo Prototype { get; } = (MethodInfo)origin;

        public void InjectContinuationPoint(ILCursor ilCursor)
        {
            var createProtagonist = CharacterDomainMethod.Call.CreateProtagonist;

            _ = ilCursor.GotoNext(x => x.MatchCallOrCallvirt(createProtagonist.GetMethodInfo()));
            ilCursor.Index++;
        }
    }

    private static async UniTask ExecuteInitial(ProtagonistCreationInfo creationInfo)
    {
        var data = new SerializableModData();
        data.Set
        (
            ModConstants.Method.ExecuteInitial.Parameters.creationInfo,
            StringSerializer.Serialize(creationInfo)
        );

        _ = await UniTaskCall.Default.CallModMethod
        (
            ModIdStr!,
            nameof(ModConstants.Method.ExecuteInitial),
            data
        );
    }

    private static async UniTask<CharacterDisplayDataForTooltip> ExecuteRoll()
    {
        var data = await UniTaskCall.Default.CallModMethod
        (
            ModIdStr!,
            nameof(ModConstants.Method.ExecuteRoll),
            new SerializableModData()
        );

        _ = data.Get
        (
            ModConstants.Method.ExecuteRoll.ReturnValue.character,
            out CharacterDisplayDataForTooltip character
        );

        return character;
    }

    private static UIElement? CharacterDisplay;
    private static Func<UI_NewGame, UniTask>? doStartNewGame;
}
