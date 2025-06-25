using System.Collections;
using GameData.Common;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;

namespace RollProtagonist.Backend;

internal class CreateProtagonistStages
{
    public CreateProtagonistStages(
        RollDelegate roll,
        AfterRollDelegate afterRoll
    )
    {
        this.roll = roll;
        this.afterRoll = afterRoll;
        stageLoop = StageLoop();
    }

    public delegate Tuple<object[], bool, object[]> RollDelegate(
        CharacterDomain instance,
        DataContext context,
        ProtagonistCreationInfo info
    );
    public delegate int AfterRollDelegate(
        CharacterDomain instance,
        DataContext context,
        ProtagonistCreationInfo info,
        object[] variables
    );

    public Character New(ProtagonistCreationInfo info)
    {
        nextStage = NextStage.New;
        creationInfo = info;

        stageLoop.MoveNext();

        throw new NotImplementedException();
    }

    public Character Roll()
    {
        nextStage = NextStage.Roll;
        stageLoop.MoveNext();

        throw new NotImplementedException();
    }

    public void Confirm()
    {
        nextStage = NextStage.AfterRoll;
        stageLoop.MoveNext();
    }

    private IEnumerator StageLoop()
    {
        object[] variables = [];

        while (true)
        {
            switch (nextStage)
            {
                case NextStage.AfterRoll:
                    {
                        afterRoll(null, null, creationInfo!, variables);

                        yield return null;

                        if (NextStage.AfterRoll == nextStage)
                        {
                            throw new Exception();
                        }
                    }
                    break;
                case NextStage.New:
                    {
                        var (_, _, newVariables) = roll(null, null, creationInfo!);
                        variables = newVariables;

                        yield return ExtractCharacter(newVariables);
                    }
                    break;
                case NextStage.Roll:
                    {
                        var (_, _, newVariables) = roll(null, null, creationInfo!);
                        variables = newVariables;

                        yield return ExtractCharacter(newVariables);
                    }
                    break;
            }
        }
    }

    private Character ExtractCharacter(object[] variables)
    {
        throw new NotImplementedException();
    }

    private enum NextStage
    {
        AfterRoll, New, Roll,
    }

    private readonly RollDelegate roll;
    private readonly AfterRollDelegate afterRoll;
    private readonly IEnumerator stageLoop;
    private NextStage nextStage = NextStage.New;
    private ProtagonistCreationInfo? creationInfo;
}