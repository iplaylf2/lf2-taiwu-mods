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

    public void ExecuteInitial(
        CharacterDomain domain,
        DataContext context,
        ProtagonistCreationInfo info
    )
    {
        characterDomain = domain;
        dataContext = context;
        creationInfo = info;
    }

    public Character ExecuteRoll()
    {
        if (characterDomain is null)
        {
            throw new InvalidOperationException("CreateProtagonist not initialized");
        }

        currentPhase = CreationPhase.Roll;

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
                case CreationPhase.Roll:
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
        Commit, Roll,
    }

    private readonly IEnumerator<StageResult> creationFlow;
    private CreationPhase currentPhase = CreationPhase.Roll;
    private CharacterDomain? characterDomain;
    private DataContext? dataContext;
    private ProtagonistCreationInfo? creationInfo;
}
