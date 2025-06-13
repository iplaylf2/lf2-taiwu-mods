using GameData.Domains.Character;
using HarmonyLib;
using GameData.Utilities;
using System.Reflection.Emit;
using System.Reflection;
using OrganizationMemberItem = Config.OrganizationMemberItem;
using OrganizationMember = Config.OrganizationMember;
using GameData.Domains.Character.Creation;
using GameData.Common;
using TiredSL.Backend.Kit;
using Transil.Operations;
using Transil.Attributes;

namespace TiredSL.Backend.InitialSetup;

[HarmonyPatch(typeof(Character), nameof(Character.OfflineCreateProtagonist))]
public static class MyHobbyValue
{
    public static bool Enabled;

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    public static MainAttributes HandleMainAttributes(
        [ConsumeStackValue] MainAttributes original,
        [InjectArgumentValue(0)] Character instance,
        [InjectArgumentValue(2)] short orgMemberId,
        [InjectArgumentValue(3)] ProtagonistCreationInfo info,
        [InjectArgumentValue(4)] DataContext context)
    {
        if (!Enabled || info.InscribedChar != null)
        {
            return original;
        }

        OrganizationMemberItem organizationMemberItem = OrganizationMember.Instance[orgMemberId];

        AdaptableLog.Info("Grade: " + organizationMemberItem.Grade);

        AdaptableLog.Info("MainAttributes original: " + string.Join(
            ',',
            organizationMemberItem.MainAttributesAdjust.Select((_, i) => original[i]))
        );

        var mainAttributesAdjust = organizationMemberItem
            .MainAttributesAdjust
            .Select(x => 0 <= x ? x : (short)RedzenHelper.SkewDistribute(context.Random, 4, 8 / 3, 2, 2, 12))
            .ToArray();

        AdaptableLog.Info("mainAttributesAdjust: " + string.Join(", ", mainAttributesAdjust));

        var result = RandomKit.NiceRetry(
            () => CharacterCreation.CreateMainAttributes(
                context.Random,
                organizationMemberItem.Grade,
                mainAttributesAdjust
            ),
            Comparer<MainAttributes>.Create((x, y) => x.GetSum().CompareTo(y.GetSum())),
            15
         );

        AdaptableLog.Info("MainAttributes: " + string.Join(
           ',',
           organizationMemberItem.MainAttributesAdjust.Select((_, i) => result[i]))
       );

        return result;
    }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    public static LifeSkillShorts HandleLifeSkillQualifications(
        [ConsumeStackValue] LifeSkillShorts original,
        [InjectArgumentValue(0)] Character instance,
        [InjectArgumentValue(2)] short orgMemberId,
        [InjectArgumentValue(3)] ProtagonistCreationInfo info,
        [InjectArgumentValue(4)] DataContext context)
    {
        if (!Enabled || info.InscribedChar != null)
        {
            return original;
        }

        OrganizationMemberItem organizationMemberItem = OrganizationMember.Instance[orgMemberId];

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
        [InjectArgumentValue(0)] Character instance,
        [InjectArgumentValue(2)] short orgMemberId,
        [InjectArgumentValue(3)] ProtagonistCreationInfo info,
        [InjectArgumentValue(4)] DataContext context)
    {
        if (!Enabled || info.InscribedChar != null)
        {
            return original;
        }

        OrganizationMemberItem organizationMemberItem = OrganizationMember.Instance[orgMemberId];

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
        if (!Enabled || info.InscribedChar != null)
        {
            return original;
        }

        return 2;
    }

    [ILHijackHandler(HijackStrategy.ReplaceOriginal)]
    public static void HandleBonusesAdd(
        [ConsumeStackValue] List<SkillQualificationBonus> bonuses,
        [ConsumeStackValue] SkillQualificationBonus origin,
        [InjectMemberValue(MemberInjectionType.Field, "_skillQualificationBonuses")]
        List<SkillQualificationBonus> targetBonuses,
        [InjectArgumentValue(3)] ProtagonistCreationInfo info,
        [InjectArgumentValue(4)] DataContext context)
    {
        if (!Enabled || info.InscribedChar != null || bonuses != targetBonuses)
        {
            bonuses.Add(origin);

            return;
        }

        AdaptableLog.Info("skillQualificationBonus origin:" + origin.Bonus);

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
        var result = origin.Bonus < niceValue.Bonus ? niceValue : origin;

        AdaptableLog.Info("skillQualificationBonus: " + result.Bonus);

        bonuses.Add(result);
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        var charType = typeof(Character).GetTypeInfo();
        
        {
            var handleMethodDict = new Dictionary<FieldInfo, Delegate>
            {
                [AccessTools.Field(charType, "_baseMainAttributes")] = HandleMainAttributes,
                [AccessTools.Field(charType, "_baseLifeSkillQualifications")] = HandleLifeSkillQualifications,
                [AccessTools.Field(charType, "_baseCombatSkillQualifications")] = HandleCombatSkillQualifications,
                [AccessTools.Field(charType, "_lifeSkillQualificationGrowthType")] = HandleGrowthType,
                [AccessTools.Field(charType, "_combatSkillQualificationGrowthType")] = HandleGrowthType
            };

            matcher
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
                        matcher.Advance(1);

                        return;
                    }


                    ILManipulator.ApplyTransformation(matcher, handleMethod, charType);

                    matcher.Advance(1);

                    AdaptableLog.Info($"handle ${field} assignment");
                }
            );
        }

        {
            var targetMethod = AccessTools.Method(
                typeof(List<SkillQualificationBonus>),
                "Add",
                [typeof(SkillQualificationBonus)]
            );

            matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Callvirt, targetMethod)
            )
            .Repeat(
                (matcher) =>
                {
                    ILManipulator.ApplyTransformation(matcher, HandleBonusesAdd, charType);

                    matcher.Advance(1);

                    AdaptableLog.Info($"handle ${targetMethod}");
                }
            );
        }

        return matcher.InstructionEnumeration();
    }
}