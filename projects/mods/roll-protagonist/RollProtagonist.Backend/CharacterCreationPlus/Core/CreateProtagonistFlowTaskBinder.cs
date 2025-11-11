using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Domains.Character.Display;
using GameData.Domains.Mod;
using LF2.Backend.Helper;
using LF2.Game.Helper.Communication;
using RollProtagonist.Common;

namespace RollProtagonist.Backend.CharacterCreationPlus.Core;

internal static class CreateProtagonistFlowTaskBinder
{
    public static void BindTaskCalls(string modId, CreateProtagonistFlow flow)
    {
        TaskCall.AddModMethod
        (
            modId,
            nameof(ModConstants.Method.ExecuteInitial),
            (context, data) =>
            {
                _ = data.Get
                (
                    ModConstants.Method.ExecuteInitial.Parameters.creationInfo,
                    out string infoString
                );
                var info = StringSerializer.Deserialize<ProtagonistCreationInfo>(infoString);

                flow.ExecuteInitial(context, info);

                return new SerializableModData();
            }
        );

        TaskCall.AddModMethod
        (
            modId,
            nameof(ModConstants.Method.ExecuteRoll),
            (context, _) =>
            {
                var character = flow.ExecuteRoll();

                var serializableModData = new SerializableModData();

                serializableModData.Set
                (
                    ModConstants.Method.ExecuteRoll.ReturnValue.character,
                    BuildCharacterDisplayData(character, flow.CreationInfo, context)
                );

                return serializableModData;
            }
        );
    }

    private static CharacterDisplayDataForTooltip BuildCharacterDisplayData
    (
        Character character,
        ProtagonistCreationInfo creationInfo,
        DataContext context
    )
    {
        return new()
        {
            Id = character.GetId(),
            TemplateId = character.GetTemplateId(),
            CreatingType = character.GetCreatingType(),
            OrganizationInfo = character.GetOrganizationInfo(),
            Age = character.GetCurrAge(),
            FullName = character.GetFullName(),
            MonkType = character.GetMonkType(),
            MonasticTitle = character.GetMonasticTitle(),
            CustomDisplayNameId =
                DomainManager.Extra.GetCharacterCustomDisplayName(character.GetId()),
            BehaviorType = BehaviorType.GetBehaviorType(character.GetBaseMorality()),
            Attraction = character.GetBaseAttraction(),
            AvatarRelatedData = new()
            {
                AvatarData = new(creationInfo.Avatar),
                DisplayAge = character.GetCurrAge(),
                ClothingDisplayId = creationInfo.ClothingTemplateId
            },
            MainAttributes = character.GetBaseMainAttributes(),
            FeatureIds = character.GetFeatureIds(),
            Gender = character.GetGender(),
            Transgender = character.GetTransgender(),
            CombatSkillQualifications = character.GetBaseCombatSkillQualifications(),
            CombatSkillQualificationGrowthType = character.GetCombatSkillQualificationGrowthType(),
            LifeSkillQualifications = character.GetBaseLifeSkillQualifications(),
            LifeSkillQualificationGrowthType = character.GetLifeSkillQualificationGrowthType(),
            Personalities = default,
            TeammateCommands =
                DomainManager.Extra.GetCharTeammateCommands(context, character.GetId()),
            NickNameId = DomainManager.Taiwu.GetFollowingNpcNickNameId(character.GetId()),
            LifeSkillAttainments = character.GetBaseLifeSkillQualifications(),
            FavorabilityToTaiwu = 0,
            IsInteractedCharacter = false
        };
    }
}
