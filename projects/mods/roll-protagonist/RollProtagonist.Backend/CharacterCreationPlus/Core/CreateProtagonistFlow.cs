using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Utilities;

namespace RollProtagonist.Backend.CharacterCreationPlus.Core;

internal sealed class CreateProtagonistFlow : IDisposable
{
    public CreateProtagonistFlow
    (
        RollOperation roll,
        CommitOperation commit
    )
    {
        creationFlow = BuildCreationFlow(roll, commit);
    }

    public delegate Tuple<object[], bool, object[]> RollOperation
    (
        CharacterDomain domain,
        DataContext context,
        ProtagonistCreationInfo info
    );
    public delegate int CommitOperation
    (
        CharacterDomain domain,
        DataContext context,
        ProtagonistCreationInfo info,
        object[] variables
    );

    public void ExecuteInitial
    (
        DataContext context,
        ProtagonistCreationInfo info
    )
    {
        dataContext = context;
        CreationInfo = info;
    }

    public Character ExecuteRoll()
    {
        if (CreationInfo is null)
        {
            throw new InvalidOperationException("CreateProtagonist not initialized");
        }

        currentPhase = CreationPhase.Roll;

        _ = creationFlow.MoveNext();

        var rollResult = (RollResult)creationFlow.Current;

        return rollResult.Character;
    }

    public int ExecuteCommit()
    {
        currentPhase = CreationPhase.Commit;

        _ = creationFlow.MoveNext();

        var commitResult = (CommitResult)creationFlow.Current;

        return commitResult.Data;
    }

    public ProtagonistCreationInfo? CreationInfo { get; private set; }

    protected abstract record PhaseResult { }
    protected record RollResult(Character Character) : PhaseResult;
    protected record CommitResult(int Data) : PhaseResult;

    private IEnumerator<PhaseResult> BuildCreationFlow(RollOperation roll, CommitOperation commit)
    {
        object[] stateVariables = [];

        while (true)
        {
            switch (currentPhase)
            {
                case CreationPhase.Commit:
                    {
                        var data = commit
                        (
                            DomainManager.Character,
                            dataContext!,
                            CreationInfo!,
                            stateVariables
                        );

                        stateVariables = [];

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
                        var (_, _, newVariables) = roll
                        (
                            DomainManager.Character,
                            dataContext!,
                            CreationInfo!
                        );

                        stateVariables = newVariables;

                        yield return new RollResult(ExtractCharacter(stateVariables));
                    }
                    break;
                default:
                    throw new InvalidProgramException();
            }
        }
    }

    private static Character ExtractCharacter(object[] variables)
    {
        return (Character)variables.Find(x => x is Character);
    }

    public void Dispose()
    {
        creationFlow.Dispose();
    }

    private enum CreationPhase
    {
        Commit = 0, Roll = 1,
    }

    private readonly IEnumerator<PhaseResult> creationFlow;
    private CreationPhase currentPhase = CreationPhase.Roll;
    private DataContext? dataContext;
}
