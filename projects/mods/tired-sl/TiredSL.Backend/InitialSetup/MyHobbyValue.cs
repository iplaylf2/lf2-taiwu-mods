using GameData.Domains.Character;
using HarmonyLib;
using GameData.Utilities;
using System.Reflection.Emit;
using System.Reflection;
using OrganizationMember = Config.OrganizationMember;
using GameData.Domains.Character.Creation;
using GameData.Common;
using TiredSL.Backend.Kit;
using Transil.Operations;
using Transil.Attributes;
using LF2.Game.Helper;

namespace TiredSL.Backend.InitialSetup;

internal static class MyHobbyValue
{
    public static bool Enabled { get; set; }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static MainAttributes HandleMainAttributes
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

        var result = RandomKit.NiceRetry
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
            "HandleMainAttributes",
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
    private static LifeSkillShorts HandleLifeSkillQualifications
    (
        [ConsumeStackValue] LifeSkillShorts original,
        [InjectArgumentValue(2)] short orgMemberId,
        [InjectArgumentValue(3)] ProtagonistCreationInfo info,
        [InjectArgumentValue(4)] DataContext context)
    {
        if (!Enabled || info.InscribedChar != null)
        {
            return original;
        }

        var organizationMemberItem = OrganizationMember.Instance[orgMemberId];

        AdaptableLog.Info("LifeSkillShorts original: " + string.Join(
            ',',
            organizationMemberItem.LifeSkillsAdjust.Select((_, i) => original[i]))
        );

        var lifeSkillsAdjust = organizationMemberItem
            .LifeSkillsAdjust
            .Select(x => 0 <= x ? x : (short)RedzenHelper.SkewDistribute(context.Random, 4, 8 / 3, 2, 2, 12))
            .ToArray();

        AdaptableLog.Info("lifeSkillsAdjust: " + string.Join(", ", lifeSkillsAdjust));

        var result = RandomKit.NiceRetry(
            () => CharacterCreation.CreateLifeSkillQualifications(
                context.Random,
                organizationMemberItem.Grade,
                lifeSkillsAdjust
            ),
            Comparer<LifeSkillShorts>.Create((x, y) => x.GetSum().CompareTo(y.GetSum())),
            15
         );

        AdaptableLog.Info("LifeSkillShorts: " + string.Join(
            ',',
            organizationMemberItem.LifeSkillsAdjust.Select((_, i) => result[i]))
        );

        return result;
    }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    public static CombatSkillShorts HandleCombatSkillQualifications(
        [ConsumeStackValue] CombatSkillShorts original,
        [InjectArgumentValue(2)] short orgMemberId,
        [InjectArgumentValue(3)] ProtagonistCreationInfo info,
        [InjectArgumentValue(4)] DataContext context)
    {
        if (!Enabled || info.InscribedChar != null)
        {
            return original;
        }

        var organizationMemberItem = OrganizationMember.Instance[orgMemberId];

        AdaptableLog.Info("CombatSkillShorts original: " + string.Join(
            ',',
            organizationMemberItem.CombatSkillsAdjust.Select((_, i) => original[i]))
        );

        var combatSkillsAdjust = organizationMemberItem
            .CombatSkillsAdjust
            .Select(x => 0 <= x ? x : (short)RedzenHelper.SkewDistribute(context.Random, 4, 8 / 3, 2, 2, 12))
            .ToArray();

        AdaptableLog.Info("combatSkillsAdjust: " + string.Join(", ", combatSkillsAdjust));

        var result = RandomKit.NiceRetry(
            () => CharacterCreation.CreateCombatSkillQualifications(
                context.Random,
                organizationMemberItem.Grade,
                combatSkillsAdjust
            ),
            Comparer<CombatSkillShorts>.Create((x, y) => x.GetSum().CompareTo(y.GetSum())),
            15
        );

        AdaptableLog.Info("CombatSkillShorts: " + string.Join(
            ',',
            organizationMemberItem.CombatSkillsAdjust.Select((_, i) => result[i]))
        );

        return result;
    }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    public static sbyte HandleGrowthType(
        [ConsumeStackValue] sbyte original,
        [InjectArgumentValue(3)] ProtagonistCreationInfo info)
    {
        return !Enabled || info.InscribedChar != null ? original : (sbyte)2;
    }

    [ILHijackHandler(HijackStrategy.ReplaceOriginal)]
    public static void HandleBonusesAdd(
        [ConsumeStackValue] IList<SkillQualificationBonus> bonuses,
        [ConsumeStackValue] SkillQualificationBonus original,
        [InjectMemberValue(MemberInjectionType.Field, nameof(Character._skillQualificationBonuses))]
        IList<SkillQualificationBonus> targetBonuses,
        [InjectArgumentValue(3)] ProtagonistCreationInfo info,
        [InjectArgumentValue(4)] DataContext context)
    {
        if (!Enabled || info.InscribedChar != null || bonuses != targetBonuses)
        {
            bonuses.Add(original);

            return;
        }

        AdaptableLog.Info("skillQualificationBonus origin:" + original.Bonus);

        var niceValue = RandomKit.NiceRetry(
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

        AdaptableLog.Info("skillQualificationBonus: " + result.Bonus);

        bonuses.Add(result);
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Character), nameof(Character.OfflineCreateProtagonist))]
    public static IEnumerable<CodeInstruction> OfflineCreateProtagonistPatch
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
                ] = HandleMainAttributes,

                [AccessTools.Field
                (charType, nameof(Character._baseLifeSkillQualifications))
                ] = HandleLifeSkillQualifications,

                [AccessTools.Field
                (charType, nameof(Character._baseCombatSkillQualifications))
                ] = HandleCombatSkillQualifications,

                [AccessTools.Field
                (charType, nameof(Character._lifeSkillQualificationGrowthType))
                ] = HandleGrowthType,

                [AccessTools.Field
                (charType, nameof(Character._combatSkillQualificationGrowthType))
                ] = HandleGrowthType
            };

            _ = matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Stfld)
            )
            .Repeat(
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

                    ILManipulator.ApplyTransformation(matcher, handleMethod, charType);

                    _ = matcher.Advance(1);

                    AdaptableLog.Info($"handle ${field} assignment");
                }
            );
        }

        {
            var targetMethod = AccessTools.Method(
                typeof(List<SkillQualificationBonus>),
                nameof(List<SkillQualificationBonus>.Add),
                [typeof(SkillQualificationBonus)]
            );

            _ = matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Callvirt, targetMethod)
            )
            .Repeat(
                (matcher) =>
                {
                    ILManipulator.ApplyTransformation(matcher, HandleBonusesAdd, charType);

                    _ = matcher.Advance(1);

                    AdaptableLog.Info($"handle ${targetMethod}");
                }
            );
        }

        return matcher.InstructionEnumeration();
    }
}