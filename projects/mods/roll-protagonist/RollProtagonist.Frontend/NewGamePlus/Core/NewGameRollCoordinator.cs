using Cysharp.Threading.Tasks;
using FrameWork;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using LF2.Game.Helper;
using LF2.Kit.Extensions;

namespace RollProtagonist.Frontend.NewGamePlus.Core;

internal sealed class NewGameRollCoordinator
(
    string modId,
    Func<UI_NewGame, Tuple<object[], bool, object[]>> beforeRoll,
    Action<UI_NewGame, object[]> afterRoll,
    UIElement characterDisplay
) : IDisposable
{
    public async UniTask Execute(UI_NewGame uiNewGame)
    {
        StructuredLogger.Info("DoStartNewGame");

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

        StructuredLogger.Info("Before roll completed");

        await CreateProtagonistAdapter.ExecuteInitial(modId, creationInfo);

        StructuredLogger.Info("Execute Initial completed");

        var isRolling = true;

        while (isRolling)
        {
            var character = await CreateProtagonistAdapter.ExecuteRoll(modId);

            var viewArg = new ArgumentBox();
            _ = viewArg.Set("Data", character);

            characterDisplay.SetOnInitArgs(viewArg);
            characterDisplay.Show();

            StructuredLogger.Info("Execute Roll completed");

            while (true)
            {
                if (CommonCommandKit.Enter.Check(characterDisplay))
                {
                    isRolling = false;

                    StructuredLogger.Info("enter");

                    break;
                }

                if (CommonCommandKit.Shift.Check(characterDisplay))
                {
                    StructuredLogger.Info("roll");

                    break;
                }

                await UniTask.NextFrame();
            }
        }

        CharacterDomainMethod.Call.CreateProtagonist(listenerId, creationInfo);

        afterRoll(uiNewGame, variables);

        StructuredLogger.Info("After roll completed");
    }

    public void Dispose()
    {
    }
}
