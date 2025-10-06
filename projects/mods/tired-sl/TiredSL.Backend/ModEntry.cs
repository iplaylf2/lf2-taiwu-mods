using GameData.Domains;
using LF2.Game.Helper.Extensions;
using TaiwuModdingLib.Core.Plugin;

namespace TiredSL.Backend;

[PluginConfig("tired-sl", "lf2", "1.0.0")]
public class ModEntry : TaiwuRemakeHarmonyPlugin, IDisposable
{
    public override void Initialize()
    {
        HarmonyInstance.PatchArray
        ([
            typeof(CombatCheat.CollapseCatchOdds),
            typeof(CombatCheat.MissMe),
            typeof(CombatCheat.FullCombatAI),

            typeof(GameOpeningCheat.AllGoodFeature),
            typeof(GameOpeningCheat.CanMoveResource),
            typeof(GameOpeningCheat.MyHobbyValue),

            typeof(NpcCheat.CherishBooks),

            typeof(SkillBreakoutCheat.BrightenUp),
            typeof(SkillBreakoutCheat.NoCostOnFailMove),
        ]);
    }

    public override void OnModSettingUpdate()
    {
        {
            var enable = CombatCheat.CollapseCatchOdds.Enabled;
            if (DomainManager.Mod.GetSetting(ModIdStr, "collapseCatchOdds", ref enable))
            {
                CombatCheat.CollapseCatchOdds.Enabled = enable;
            }
        }
        {
            var enable = CombatCheat.FullCombatAI.Enabled;
            if (DomainManager.Mod.GetSetting(ModIdStr, "fullCombatAI", ref enable))
            {
                CombatCheat.FullCombatAI.Enabled = enable;
            }
        }
        {
            var enable = CombatCheat.MissMe.Enabled;
            if (DomainManager.Mod.GetSetting(ModIdStr, "missMe", ref enable))
            {
                CombatCheat.MissMe.Enabled = enable;
            }
        }
        {
            var enable = GameOpeningCheat.AllGoodFeature.Enabled;
            if (DomainManager.Mod.GetSetting(ModIdStr, "allGoodFeature", ref enable))
            {
                GameOpeningCheat.AllGoodFeature.Enabled = enable;
            }
        }
        {
            var enable = GameOpeningCheat.CanMoveResource.Enabled;
            if (DomainManager.Mod.GetSetting(ModIdStr, "canMoveResource", ref enable))
            {
                GameOpeningCheat.CanMoveResource.Enabled = enable;
            }
        }
        {
            var enable = GameOpeningCheat.MyHobbyValue.Enabled;
            if (DomainManager.Mod.GetSetting(ModIdStr, "myHobbyValue", ref enable))
            {
                GameOpeningCheat.MyHobbyValue.Enabled = enable;
            }
        }
        {
            var enable = NpcCheat.CherishBooks.Enabled;
            if (DomainManager.Mod.GetSetting(ModIdStr, "cherishBooks", ref enable))
            {
                NpcCheat.CherishBooks.Enabled = enable;
            }
        }
        {
            var enable = SkillBreakoutCheat.BrightenUp.Enabled;
            if (DomainManager.Mod.GetSetting(ModIdStr, "brightenUp", ref enable))
            {
                SkillBreakoutCheat.BrightenUp.Enabled = enable;
            }
        }
        {
            var enable = SkillBreakoutCheat.NoCostOnFailMove.Enabled;
            if (DomainManager.Mod.GetSetting(ModIdStr, "noCostOnFailMove", ref enable))
            {
                SkillBreakoutCheat.NoCostOnFailMove.Enabled = enable;
            }
        }
    }
}
