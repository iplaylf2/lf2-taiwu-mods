using Cysharp.Threading.Tasks;
using GameData.Domains.Character.Creation;
using GameData.Domains.Character.Display;
using GameData.Domains.Mod;
using LF2.Frontend.Helper;
using LF2.Game.Helper.Communication;
using RollProtagonist.Common;

namespace RollProtagonist.Frontend.NewGamePlus.Core;

internal static class CreateProtagonistFlow
{
    public static async UniTask ExecuteInitial(string modId, ProtagonistCreationInfo creationInfo)
    {
        var data = new SerializableModData();
        data.Set
        (
            ModConstants.Method.ExecuteInitial.Parameters.creationInfo,
            StringSerializer.Serialize(creationInfo)
        );

        _ = await UniTaskCall.Default.CallModMethod
        (
            modId,
            nameof(ModConstants.Method.ExecuteInitial),
            data
        );
    }

    public static async UniTask<CharacterDisplayDataForTooltip> ExecuteRoll(string modId)
    {
        var data = await UniTaskCall.Default.CallModMethod
        (
            modId,
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
}
