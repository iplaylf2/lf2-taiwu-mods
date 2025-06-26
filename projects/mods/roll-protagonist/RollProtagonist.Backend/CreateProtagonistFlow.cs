using GameData.Common;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Utilities;

namespace RollProtagonist.Backend;

internal class CreateProtagonistFlow
{
    public CreateProtagonistFlow(
        RollOperation roll,
        CommitOperation complete
    )
    {
        creationFlow = BuildCreationFlow(roll, complete);
    }

    public delegate Tuple<object[], bool, object[]> RollOperation(
        CharacterDomain domain,
        DataContext context,
        ProtagonistCreationInfo info
    );
    public delegate int CommitOperation(
        CharacterDomain domain,
        DataContext context,
        ProtagonistCreationInfo info,
        object[] variables
    );

    public Character ExecuteInitialRoll(
        CharacterDomain domain,
        DataContext context,
        ProtagonistCreationInfo info
    )
    {
        currentPhase = CreationPhase.InitialRoll;
        characterDomain = domain;
        dataContext = context;
        creationInfo = info;

        creationFlow.MoveNext();

        var rollResult = (RollResult)creationFlow.Current;

        return rollResult.Character;
    }

    public Character ExecuteReroll()
    {
        if (characterDomain is null)
        {
            throw new InvalidOperationException("Character domain not initialized");
        }

        currentPhase = CreationPhase.Reroll;

        creationFlow.MoveNext();

        var rollResult = (RollResult)creationFlow.Current;

        return rollResult.Character;
    }

    public void CommitCreation()
    {
        currentPhase = CreationPhase.Commit;
        creationFlow.MoveNext();
    }

    protected abstract record StageResult { }
    protected record RollResult(Character Character) : StageResult { }
    protected record CompleteRollResult : StageResult { }

    private IEnumerator<StageResult> BuildCreationFlow(RollOperation roll, CommitOperation completeRoll)
    {
        object[] stateVariables = [];

        while (true)
        {
            switch (currentPhase)
            {
                case CreationPhase.Commit:
                    {
                        completeRoll(characterDomain!, dataContext!, creationInfo!, stateVariables);

                        yield return new CompleteRollResult();

                        if (CreationPhase.Commit == currentPhase)
                        {
                            throw new InvalidOperationException(
                                "Must perform at least one roll operation before committing"
                            );
                        }
                    }
                    break;
                case CreationPhase.InitialRoll:
                    {
                        var (_, _, newVariables) = roll(characterDomain!, dataContext!, creationInfo!);
                        stateVariables = newVariables;

                        yield return new RollResult(ExtractCharacter(stateVariables));
                    }
                    break;
                case CreationPhase.Reroll:
                    {
                        var (_, _, newVariables) = roll(characterDomain!, dataContext!, creationInfo!);
                        stateVariables = newVariables;

                        yield return new RollResult(ExtractCharacter(stateVariables));
                    }
                    break;
            }
        }
    }

    private static Character ExtractCharacter(object[] variables)
    {
        return (Character)variables.Find(x => x is Character);
    }

    private enum CreationPhase
    {
        Commit, InitialRoll, Reroll,
    }

    private readonly IEnumerator<StageResult> creationFlow;
    private CreationPhase currentPhase = CreationPhase.InitialRoll;
    private CharacterDomain? characterDomain;
    private DataContext? dataContext;
    private ProtagonistCreationInfo? creationInfo;
}
