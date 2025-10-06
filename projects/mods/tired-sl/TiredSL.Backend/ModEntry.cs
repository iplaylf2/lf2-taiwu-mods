using GameData.Domains;
using TaiwuModdingLib.Core.Plugin;

namespace TiredSL.Backend;

[PluginConfig("tired-sl", "lf2", "1.0.0")]
public class ModEntry : TaiwuRemakeHarmonyPlugin, IDisposable
{
    public override void Initialize()
    {
        HarmonyInstance.PatchAll(typeof(CombatCheat.CollapseCatchOdds));
        HarmonyInstance.PatchAll(typeof(CombatCheat.MissMe));
        HarmonyInstance.PatchAll(typeof(CombatCheat.FullCombatAI));
        HarmonyInstance.PatchAll(typeof(InitialSetup.AllGoodFeature));
        HarmonyInstance.PatchAll(typeof(InitialSetup.CanMoveResource));
        HarmonyInstance.PatchAll(typeof(InitialSetup.MyHobbyValue));
        HarmonyInstance.PatchAll(typeof(SkillBreakout.BrightenUp));
        HarmonyInstance.PatchAll(typeof(SkillBreakout.NoCostOnFailMove));
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
            var enable = InitialSetup.AllGoodFeature.Enabled;
            if (
                DomainManager.Mod.GetSetting(ModIdStr, "allGoodFeature", ref enable)
                && InitialSetup.AllGoodFeature.Enabled != enable
            )
            {
                InitialSetup.AllGoodFeature.Enabled = enable;
            }
        }
        {
            var enable = InitialSetup.CanMoveResource.Enabled;
            if (
                DomainManager.Mod.GetSetting(ModIdStr, "canMoveResource", ref enable)
                && InitialSetup.CanMoveResource.Enabled != enable
            )
            {
                InitialSetup.CanMoveResource.Enabled = enable;
            }
        }
        {
            var enable = InitialSetup.MyHobbyValue.Enabled;
            if (
                DomainManager.Mod.GetSetting(ModIdStr, "myHobbyValue", ref enable)
                && InitialSetup.MyHobbyValue.Enabled != enable
            )
            {
                InitialSetup.MyHobbyValue.Enabled = enable;
            }
        }
        {
            var enable = SkillBreakout.BrightenUp.Enabled;
            if (
                DomainManager.Mod.GetSetting(ModIdStr, "brightenUp", ref enable)
                && SkillBreakout.BrightenUp.Enabled != enable
            )
            {
                SkillBreakout.BrightenUp.Enabled = enable;
            }
        }
        {
            var enable = SkillBreakout.NoCostOnFailMove.Enabled;
            if (
                DomainManager.Mod.GetSetting(ModIdStr, "noCostOnFailMove", ref enable)
                && SkillBreakout.NoCostOnFailMove.Enabled != enable
            )
            {
                SkillBreakout.NoCostOnFailMove.Enabled = enable;
            }
        }
    }
}
