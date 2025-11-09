using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Domains.Character.Display;
using GameData.Domains.Mod;
using HarmonyLib;
using LF2.Backend.Helper;
using LF2.Cecil.Helper;
using LF2.Game.Helper.Communication;
using MonoMod.Cil;
using RollProtagonist.Common;
using System.Reflection;
using RollProtagonist.Backend.CharacterCreationPlus.Core;
using LF2.Game.Helper;
using LF2.Kit.Service;
using System.Diagnostics.CodeAnalysis;

namespace RollProtagonist.Backend.CharacterCreationPlus.Patches;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
[HarmonyPatch(typeof(CharacterDomain))]
internal static class CharacterDomainPatch
{
    [HarmonyILManipulator]
    [HarmonyPatch(nameof(CharacterDomain.CreateProtagonist))]
    private static void CreateProtagonistTap(MethodBase originMethod)
    {
        StructuredLogger.Info("CreateProtagonistTap started");

        var offlineCreateProtagonist =
            AccessTools.Method(typeof(Character), nameof(Character.OfflineCreateProtagonist));

        IEnumerable<Type> RollOperationSplitPoint(ILCursor ilCursor)
        {
            _ = ilCursor.GotoNext
            (
                MoveType.After,
                x => x.MatchCallOrCallvirt(offlineCreateProtagonist),
                x => x.MatchStloc(out var _)
            );

            return [];
        }

        void CommitOperationContinuationPoint(ILCursor ilCursor)
        {
            _ = ilCursor.GotoNext
            (
                MoveType.After,
                x => x.MatchCallOrCallvirt(offlineCreateProtagonist),
                x => x.MatchStloc(out var _)
            );
        }

        var roll = MethodSegmenter.CreateLeftSegment<CreateProtagonistFlow.RollOperation>
        (
            (MethodInfo)originMethod,
            RollOperationSplitPoint
        );

        StructuredLogger.Info("method generated", new { method = nameof(roll) });

        var commit = MethodSegmenter.CreateRightSegment<CreateProtagonistFlow.CommitOperation>
        (
            (MethodInfo)originMethod,
            CommitOperationContinuationPoint
        );

        StructuredLogger.Info("method generated", new { method = nameof(commit) });

        var creationFlow = ModServiceRegistry.Add(new CreateProtagonistFlow(roll, commit));

        _ = ModServiceRegistry.TryGet(out ModConfig? config);

        TaskCall.AddModMethod
        (
            config!.ModId,
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
            config!.ModId,
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
    [HarmonyPatch(nameof(CharacterDomain.CreateProtagonist))]
    private static bool CreateProtagonistPrefix(ref int __result)
    {
        _ = ModServiceRegistry.TryGet(out CreateProtagonistFlow? flow);

        __result = flow!.ExecuteCommit();

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
}
