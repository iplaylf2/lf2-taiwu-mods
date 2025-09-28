using CharacterFeature = Config.CharacterFeature;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Utilities;
using HarmonyLib;
using Redzen.Random;
using System.Reflection.Emit;
using Transil.Attributes;
using Transil.Operations;

namespace TiredSL.Backend.InitialSetup;

public static class AllGoodFeature
{
    public static bool Enabled { get; set; }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    public static bool HandleAllGoodBasicFeature(
       [ConsumeStackValue] bool original,
       [InjectArgumentValue(0)] ref FeatureCreationContext context
    )
    {
        return (Enabled && context.IsProtagonist) || original;
    }

    [HarmonyPatch(typeof(CharacterCreation), nameof(CharacterCreation.ApplyFeatureIds))]
    [HarmonyPrefix]
    public static void PrefixApplyFeatureIds(ref FeatureCreationContext context, Dictionary<short, short> featureGroup2Id)
    {
        if (!Enabled || !context.IsProtagonist)
        {
            return;
        }

        if (
            !context.RandomFeaturesAtCreating
            || featureGroup2Id.ContainsKey(CharacterFeature.DefKey.OneYearOldCatch0)
            || 1 > context.CurrAge
        )
        {
            return;
        }

        AdaptableLog.Info("GenerateOneYearOldCatchFeature");

        CharacterCreation.AddFeature
        (
            featureGroup2Id,
            CharacterDomain.GenerateOneYearOldCatchFeature(RandomDefaults.CreateRandomSource())
        );
    }

    [HarmonyPatch(typeof(CharacterCreation), nameof(CharacterCreation.GenerateRandomBasicFeatures))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> GenerateRandomBasicFeatures(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        var targetField = AccessTools.Field(typeof(FeatureCreationContext), nameof(FeatureCreationContext.AllGoodBasicFeature));

        matcher
        .Start()
        .MatchForward(
            false,
            new CodeMatch(OpCodes.Ldfld, targetField)
        )
        .Repeat(
            (matcher) =>
            {
                matcher.Advance(1);

                ILManipulator.ApplyTransformation(matcher, HandleAllGoodBasicFeature);

                matcher.Advance(1);

                AdaptableLog.Info($"handle ${targetField} access");
            }
        );

        return matcher.InstructionEnumeration();
    }
}