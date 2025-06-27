using GameData.Common;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Utilities;

namespace RollProtagonist.Backend;

internal class CreateProtagonistFlow
{
    public CreateProtagonistFlow(
        RollOperation roll,
        CommitOperation commit
    )
    {
        creationFlow = BuildCreationFlow(roll, commit);
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

    public int ExecuteCommit()
    {
        currentPhase = CreationPhase.Commit;

        creationFlow.MoveNext();

        var commitResult = (CommitResult)creationFlow.Current;

        return commitResult.Data;
    }

    protected abstract record PhaseResult { }
    protected record RollResult(Character Character) : PhaseResult { }
    protected record CommitResult(int Data) : PhaseResult { }

    private IEnumerator<PhaseResult> BuildCreationFlow(RollOperation roll, CommitOperation commit)
    {
        object[] stateVariables = [];

        while (true)
        {
            switch (currentPhase)
            {
                case CreationPhase.Commit:
                    {
                        var data = commit(characterDomain!, dataContext!, creationInfo!, stateVariables);

                        yield return new CommitResult(data);

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

    private readonly IEnumerator<PhaseResult> creationFlow;
    private CreationPhase currentPhase = CreationPhase.Roll;
    private CharacterDomain? characterDomain;
    private DataContext? dataContext;
    private ProtagonistCreationInfo? creationInfo;
}
