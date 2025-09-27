using HarmonyLib;
using GameData.Utilities;
using System.Reflection.Emit;
using GameData.Domains.Building;
using GameData.Domains;
using Config;
using GameData.Domains.Map;
using Transil.Attributes;
using Transil.Operations;

namespace TiredSL.Backend.InitialSetup;

[HarmonyPatch(typeof(BuildingDomain), nameof(BuildingDomain.CreateBuildingArea))]
public static class CanMoveResource
{
    public static bool Enabled { get; set; }

    [ILHijackHandler(HijackStrategy.InsertAdditional)]
    public static BuildingBlockData HandleBlockDataNew(
        [ConsumeStackValue] BuildingBlockData original,
        [InjectArgumentValue(2)] short mapAreaId,
        [InjectArgumentValue(3)] short mapBlockId
    )
    {
        var taiwuSettlementId = DomainManager.Taiwu.GetTaiwuVillageSettlementId();
        var settlement = DomainManager.Organization.GetSettlementByLocation(new Location(mapAreaId, mapBlockId));

        if (
            !Enabled
            || taiwuSettlementId != settlement.GetId()
         )
        {
            return original;
        }

        if (
            BuildingBlock.Instance[original.TemplateId] is { } block
            && BuildingBlockData.IsUsefulResource(block.Type)
            && original.Level == 1)
        {
            original.Level = 2;

            AdaptableLog.Info("Upgrade level:" + block.Name);
        }

        return original;
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        var targetCtor = AccessTools.Constructor(
            typeof(BuildingBlockData),
            [typeof(short), typeof(short), typeof(sbyte), typeof(short)]
        );

        matcher
        .MatchForward(
            false,
            new CodeMatch(OpCodes.Newobj, targetCtor)
        )
        .Repeat(
            (matcher) =>
            {
                matcher.Advance(1);

                ILManipulator.ApplyTransformation(matcher, HandleBlockDataNew);

                matcher.Advance(1);

                AdaptableLog.Info($"handle {targetCtor} new");
            }
        );

        return matcher.InstructionEnumeration();
    }
}