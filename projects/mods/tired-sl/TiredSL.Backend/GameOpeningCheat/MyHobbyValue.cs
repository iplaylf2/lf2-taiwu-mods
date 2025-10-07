using GameData.Common;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Utilities;
using HarmonyLib;
using LF2.Game.Helper;
using LF2.Kit.Random;
using OrganizationMember = Config.OrganizationMember;
using System.Reflection;
using System.Reflection.Emit;
using Transil.Attributes;
using Transil.Operations;

namespace TiredSL.Backend.GameOpeningCheat;

internal static class MyHobbyValue
{
    public static bool Enabled { get; set; }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static MainAttributes HandleMainAttributesValue
    (
        [ConsumeStackValue] MainAttributes original,
        [InjectArgumentValue(2)] short orgMemberId,
        [InjectArgumentValue(3)] ProtagonistCreationInfo info,
        [InjectArgumentValue(4)] DataContext context
    )
    {
        if (!Enabled || info.InscribedChar != null)
        {
            return original;
        }

        var organizationMemberItem = OrganizationMember.Instance[orgMemberId];

        var mainAttributesAdjust = organizationMemberItem
        .MainAttributesAdjust
        .Select(x => 0 <= x ? x : (short)RedzenHelper.SkewDistribute(context.Random, 4, 8 / 3, 2, 2, 12))
        .ToArray();

        var result = RollHelper.RetryAndCompare
        (
            () => CharacterCreation.CreateMainAttributes
            (
                context.Random,
                organizationMemberItem.Grade,
                mainAttributesAdjust
            ),
            Comparer<MainAttributes>.Create((x, y) => x.GetSum().CompareTo(y.GetSum())),
            15
        );

        StructuredLogger.Info
        (
            "MainAttributes",
            new
            {
                actual = string.Join
                (
                   ',',
                   organizationMemberItem.MainAttributesAdjust.Select((_, i) => result[i])
                ),
                attributesAdjust = string.Join(", ", mainAttributesAdjust),
                organizationMemberItem.Grade,
                original = string.Join
                (
                    ',',
                    organizationMemberItem.MainAttributesAdjust.Select((_, i) => original[i])
                )
            }
        );

        return result;
    }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static LifeSkillShorts HandleLifeSkillQualificationsValue
    (
        [ConsumeStackValue] LifeSkillShorts original,
        [InjectArgumentValue(2)] short orgMemberId,
        [InjectArgumentValue(3)] ProtagonistCreationInfo info,
        [InjectArgumentValue(4)] DataContext context
    )
    {
        if (!Enabled || info.InscribedChar != null)
        {
            return original;
        }

        var organizationMemberItem = OrganizationMember.Instance[orgMemberId];

        var lifeSkillsAdjust = organizationMemberItem
        .LifeSkillsAdjust
        .Select(x => 0 <= x ? x : (short)RedzenHelper.SkewDistribute(context.Random, 4, 8 / 3, 2, 2, 12))
        .ToArray();

        var result = RollHelper.RetryAndCompare
        (
            () => CharacterCreation.CreateLifeSkillQualifications
            (
                context.Random,
                organizationMemberItem.Grade,
                lifeSkillsAdjust
            ),
            Comparer<LifeSkillShorts>.Create((x, y) => x.GetSum().CompareTo(y.GetSum())),
            15
        );

        StructuredLogger.Info
        (
            "LifeSkillQualifications",
            new
            {
                actual = string.Join
                (
                   ',',
                   organizationMemberItem.LifeSkillsAdjust.Select((_, i) => result[i])
                ),
                attributesAdjust = string.Join(", ", lifeSkillsAdjust),
                organizationMemberItem.Grade,
                original = string.Join
                (
                    ',',
                    organizationMemberItem.LifeSkillsAdjust.Select((_, i) => original[i])
                )
            }
        );

        return result;
    }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static CombatSkillShorts HandleCombatSkillQualificationsValue
    (
        [ConsumeStackValue] CombatSkillShorts original,
        [InjectArgumentValue(2)] short orgMemberId,
        [InjectArgumentValue(3)] ProtagonistCreationInfo info,
        [InjectArgumentValue(4)] DataContext context
    )
    {
        if (!Enabled || info.InscribedChar != null)
        {
            return original;
        }

        var organizationMemberItem = OrganizationMember.Instance[orgMemberId];

        var combatSkillsAdjust = organizationMemberItem
        .CombatSkillsAdjust
        .Select(x => 0 <= x ? x : (short)RedzenHelper.SkewDistribute(context.Random, 4, 8 / 3, 2, 2, 12))
        .ToArray();

        var result = RollHelper.RetryAndCompare
        (
            () => CharacterCreation.CreateCombatSkillQualifications
            (
                context.Random,
                organizationMemberItem.Grade,
                combatSkillsAdjust
            ),
            Comparer<CombatSkillShorts>.Create((x, y) => x.GetSum().CompareTo(y.GetSum())),
            15
        );

        StructuredLogger.Info
        (
            "CombatSkillQualifications",
            new
            {
                actual = string.Join
                (
                   ',',
                   organizationMemberItem.CombatSkillsAdjust.Select((_, i) => result[i])
                ),
                attributesAdjust = string.Join(", ", combatSkillsAdjust),
                organizationMemberItem.Grade,
                original = string.Join
                (
                    ',',
                    organizationMemberItem.CombatSkillsAdjust.Select((_, i) => original[i])
                )
            }
        );

        return result;
    }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static sbyte HandleGrowthTypeValue
    (
        [ConsumeStackValue] sbyte original,
        [InjectArgumentValue(3)] ProtagonistCreationInfo info
    )
    {
        return !Enabled || info.InscribedChar != null
        ? original
        : SkillQualificationGrowthType.LateBlooming;
    }

    [ILHijackHandler(HijackStrategy.ReplaceOriginal)]
    private static void HandleBonusesAdd
    (
        [ConsumeStackValue] IList<SkillQualificationBonus> bonuses,
        [ConsumeStackValue] SkillQualificationBonus original,
        [InjectArgumentValue(0)] Character instance,
        [InjectArgumentValue(3)] ProtagonistCreationInfo info,
        [InjectArgumentValue(4)] DataContext context
    )
    {
        if (!Enabled || info.InscribedChar != null || bonuses != instance._skillQualificationBonuses)
        {
            bonuses.Add(original);

            return;
        }

        var niceValue = RollHelper.RetryAndCompare
        (
            () =>
            {
                var random = context.Random;
                int num = random.Next(2);
                int num2 = (num == 0) ? random.Next(16) : random.Next(14);
                int num3 = RedzenHelper.SkewDistribute(context.Random, 7f, 1.5f, 2f, 3, 15);
                return new SkillQualificationBonus((sbyte)num, (sbyte)num2, (sbyte)num3, -1);
            },
            Comparer<SkillQualificationBonus>.Create((x, y) => x.Bonus.CompareTo(y.Bonus)),
            15
        );

        var result = original.Bonus < niceValue.Bonus ? niceValue : original;

        bonuses.Add(result);

        StructuredLogger.Info
        (
            "SkillQualificationBonus",
            new { actual = result.Bonus, original = original.Bonus }
        );
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Character), nameof(Character.OfflineCreateProtagonist))]
    private static IEnumerable<CodeInstruction> OfflineCreateProtagonistPatch
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        var matcher = new CodeMatcher(instructions);

        var charType = typeof(Character).GetTypeInfo();

        {
            var handleMethodDict = new Dictionary<FieldInfo, Delegate>
            {
                [AccessTools.Field
                (charType, nameof(Character._baseMainAttributes))
                ] = HandleMainAttributesValue,

                [AccessTools.Field
                (charType, nameof(Character._baseLifeSkillQualifications))
                ] = HandleLifeSkillQualificationsValue,

                [AccessTools.Field
                (charType, nameof(Character._baseCombatSkillQualifications))
                ] = HandleCombatSkillQualificationsValue,

                [AccessTools.Field
                (charType, nameof(Character._lifeSkillQualificationGrowthType))
                ] = HandleGrowthTypeValue,

                [AccessTools.Field
                (charType, nameof(Character._combatSkillQualificationGrowthType))
                ] = HandleGrowthTypeValue
            };

            _ = matcher
            .Start()
            .MatchStartForward(new CodeMatch(OpCodes.Stfld))
            .Repeat
            (
                (matcher) =>
                {
                    if (
                        matcher.Instruction.operand is not FieldInfo field
                        || !handleMethodDict.TryGetValue(field, out var handleMethod)
                    )
                    {
                        _ = matcher.Advance(1);

                        return;
                    }

                    ILManipulator.ApplyTransformation(matcher, handleMethod);

                    _ = matcher.Advance(1);

                    StructuredLogger.Info("handleStfld", new { field = field.Name });
                }
            );
        }

        {
            var targetMethod = AccessTools.Method
            (
                typeof(List<SkillQualificationBonus>),
                nameof(List<SkillQualificationBonus>.Add),
                [typeof(SkillQualificationBonus)]
            );

            _ = matcher
            .Start()
            .MatchStartForward(new CodeMatch(OpCodes.Callvirt, targetMethod))
            .Repeat
            (
                (matcher) =>
                {
                    ILManipulator.ApplyTransformation(matcher, HandleBonusesAdd);

                    _ = matcher.Advance(1);

                    StructuredLogger.Info("HandleBonusesAdd");
                }
            );
        }

        return matcher.InstructionEnumeration();
    }
}