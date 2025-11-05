using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Domains.Character.Display;
using GameData.Domains.Mod;
using GameData.Utilities;
using HarmonyLib;
using LF2.Backend.Helper;
using LF2.Cecil.Helper;
using LF2.Game.Helper.Communication;
using MonoMod.Cil;
using RollProtagonist.Common;
using System.Reflection;
using RollProtagonist.Backend.CharacterCreationPlus.Core;

namespace RollProtagonist.Backend.CharacterCreationPlus.Patching;

[HarmonyPatch(typeof(CharacterDomain), nameof(CharacterDomain.CreateProtagonist))]
internal static class CreateProtagonistPatch
{
    public static string? ModIdStr { get; set; }

    [HarmonyILManipulator]
    private static void BuildCreationFlow(MethodBase origin)
    {
        AdaptableLog.Info("CreateProtagonist patch started");

        static IEnumerable<Type> RollOperationSplitPoint(ILCursor ilCursor)
        {
            var offlineCreateProtagonist =
            AccessTools.Method(typeof(Character), nameof(Character.OfflineCreateProtagonist));

            _ = ilCursor.GotoNext
            (
                MoveType.After,
                x => x.MatchCallOrCallvirt(offlineCreateProtagonist),
                x => x.MatchStloc(out var _)
            );

            return [];
        }

        static void CommitOperationContinuationPoint(ILCursor ilCursor)
        {
            var offlineCreateProtagonist =
            AccessTools.Method(typeof(Character), nameof(Character.OfflineCreateProtagonist));

            _ = ilCursor.GotoNext
            (
                MoveType.After,
                x => x.MatchCallOrCallvirt(offlineCreateProtagonist),
                x => x.MatchStloc(out var _)
            );
        }

        var roll = MethodSegmenter.CreateLeftSegment<CreateProtagonistFlow.RollOperation>
        (
            (MethodInfo)origin,
            RollOperationSplitPoint
        );

        AdaptableLog.Info($"{nameof(roll)} generated");

        var commit = MethodSegmenter.CreateRightSegment<CreateProtagonistFlow.CommitOperation>
        (
            (MethodInfo)origin,
            CommitOperationContinuationPoint
        );

        AdaptableLog.Info($"{nameof(commit)} generated");

        creationFlow = new CreateProtagonistFlow(roll, commit);

        TaskCall.AddModMethod
        (
            ModIdStr!,
            nameof(ModConstants.Method.ExecuteInitial),
            (context, data) =>
            {
                _ = data.Get
                (
                    ModConstants.Method.ExecuteInitial.Parameters.creationInfo,
                    out string infoString
                );
                var info = StringSerializer.Deserialize<ProtagonistCreationInfo>(infoString);

                creationFlow.ExecuteInitial(context, info!);

                return new SerializableModData();
            }
        );

        TaskCall.AddModMethod
        (
            ModIdStr!,
            nameof(ModConstants.Method.ExecuteRoll),
            (context, _) =>
            {
                var character = creationFlow.ExecuteRoll();

                var serializableModData = new SerializableModData();

                serializableModData.Set
                (
                    ModConstants.Method.ExecuteRoll.ReturnValue.character,
                    BuildCharacterDisplayData(character, creationFlow.CreationInfo!, context)
                );

                return serializableModData;
            }
        );
    }

    [HarmonyPrefix]
    private static bool CreateProtagonist(ref int __result)
    {
        __result = creationFlow!.ExecuteCommit();

        return false;
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
                AvatarData = new(creationInfo!.Avatar),
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

    private static CreateProtagonistFlow? creationFlow;
}
