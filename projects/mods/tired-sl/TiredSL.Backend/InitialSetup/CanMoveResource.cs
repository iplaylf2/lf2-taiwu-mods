using HarmonyLib;
using System.Reflection.Emit;
using GameData.Domains.Building;
using GameData.Domains;
using Config;
using GameData.Domains.Map;
using Transil.Attributes;
using Transil.Operations;
using LF2.Game.Helper;

namespace TiredSL.Backend.InitialSetup;

internal static class CanMoveResource
{
    public static bool Enabled { get; set; }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    private static BuildingBlockData HandleBlockDataNew
    (
        [ConsumeStackValue] BuildingBlockData original,
        [InjectArgumentValue(2)] short mapAreaId,
        [InjectArgumentValue(3)] short mapBlockId
    )
    {
        if (!Enabled)
        {
            return original;
        }

        var taiwuSettlementId = DomainManager.Taiwu.GetTaiwuVillageSettlementId();
        var settlement = DomainManager.Organization.GetSettlementByLocation
        (
            new Location(mapAreaId, mapBlockId)
        );

        if (taiwuSettlementId != settlement.GetId())
        {
            return original;
        }

        if
        (
            BuildingBlock.Instance[original.TemplateId] is { } block
            && BuildingBlockData.IsUsefulResource(block.Type)
            && original.Level == 1
        )
        {
            original.Level = 2;

            StructuredLogger.Info("Upgrade level", new { block.Name });
        }

        return original;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(BuildingDomain), nameof(BuildingDomain.CreateBuildingArea))]
    private static IEnumerable<CodeInstruction> CreateBuildingAreaPatch
    (
        IEnumerable<CodeInstruction> instructions
    )
    {
        var matcher = new CodeMatcher(instructions);

        var targetCtor = AccessTools.Constructor
        (
            typeof(BuildingBlockData),
            [typeof(short), typeof(short), typeof(sbyte), typeof(short)]
        );

        _ = matcher
        .MatchForward
        (
            false,
            new CodeMatch(OpCodes.Newobj, targetCtor)
        )
        .Repeat
        (
            (matcher) =>
            {
                _ = matcher.Advance(1);

                ILManipulator.ApplyTransformation(matcher, HandleBlockDataNew);

                _ = matcher.Advance(1);

                StructuredLogger.Info("HandleBlockDataNew");
            }
        );

        return matcher.InstructionEnumeration();
    }
}