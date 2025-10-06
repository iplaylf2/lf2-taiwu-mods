using CharacterFeature = Config.CharacterFeature;
using GameData.Common;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using HarmonyLib;
using LF2.Game.Helper;
using System.Reflection.Emit;
using Transil.Attributes;
using Transil.Operations;

namespace TiredSL.Backend.GameOpeningCheat;

internal static class AllGoodFeature
{
    public static bool Enabled { get; set; }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static bool HandleAllGoodBasicFeatureResult
    (
       [ConsumeStackValue] bool original,
       [InjectArgumentValue(0)] ref FeatureCreationContext context
    )
    {
        return (Enabled && context.IsProtagonist) || original;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterCreation), nameof(CharacterCreation.ApplyFeatureIds))]
    private static void ApplyFeatureIdsPatch
    (
        ref FeatureCreationContext context,
        Dictionary<short, short> featureGroup2Id
    )
    {
        if (!Enabled || !context.IsProtagonist)
        {
            return;
        }

        if
        (
            !context.RandomFeaturesAtCreating
            || featureGroup2Id.ContainsKey(CharacterFeature.DefKey.OneYearOldCatch0)
            || context.CurrAge < 1
        )
        {
            return;
        }

        var dataContext = DataContextManager.GetCurrentThreadDataContext();

        CharacterCreation.AddFeature
        (
            featureGroup2Id,
            CharacterDomain.GenerateOneYearOldCatchFeature(dataContext.Random)
        );

        StructuredLogger.Info("GenerateOneYearOldCatchFeature");
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(CharacterCreation), nameof(CharacterCreation.GenerateRandomBasicFeatures))]
    private static IEnumerable<CodeInstruction> GenerateRandomBasicFeaturesPatch
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        var matcher = new CodeMatcher(instructions);

        var targetField = AccessTools.Field
        (
            typeof(FeatureCreationContext),
            nameof(FeatureCreationContext.AllGoodBasicFeature)
        );

        _ = matcher
        .Start()
        .MatchForward
        (
            false,
            new CodeMatch(OpCodes.Ldfld, targetField)
        )
        .Repeat
        (
            (matcher) =>
            {
                _ = matcher.Advance(1);

                ILManipulator.ApplyTransformation(matcher, HandleAllGoodBasicFeatureResult);

                _ = matcher.Advance(1);

                StructuredLogger.Info("HandleAllGoodBasicFeatureResult");
            }
        );

        return matcher.InstructionEnumeration();
    }
}