using GameData.Domains;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;

namespace TiredSL.Backend;

[PluginConfig("tired-sl", "lf2", "1.0.0")]
public class BackendExport : TaiwuRemakeHarmonyPlugin, IDisposable
{
    private Harmony? harmony;

    public override void Dispose()
    {
        harmony?.UnpatchSelf();
    }

    public override void Initialize()
    {
        harmony = new Harmony(ModIdStr);

        harmony.PatchAll(typeof(Random.RandomPatch));

        harmony.PatchAll(typeof(Combat.CollapseCatchOdds));
        harmony.PatchAll(typeof(Combat.MissMe));
        harmony.PatchAll(typeof(Combat.FullCombatAI));
        harmony.PatchAll(typeof(InitialSetup.AllGoodFeature));
        harmony.PatchAll(typeof(InitialSetup.CanMoveResource));
        harmony.PatchAll(typeof(InitialSetup.MyHobbyValue));
        harmony.PatchAll(typeof(SkillBreakout.BrightenUp));
        harmony.PatchAll(typeof(SkillBreakout.EndlessStep));
    }

    public override void OnModSettingUpdate()
    {
        {
            var enable = Combat.CollapseCatchOdds.Enabled;
            if (
                DomainManager.Mod.GetSetting(ModIdStr, "niceCatch", ref enable)
                && Combat.CollapseCatchOdds.Enabled != enable
            )
            {
                Combat.CollapseCatchOdds.Enabled = enable;
            }
        }
        {
            var enable = Combat.FullCombatAI.Enabled;
            if (
                DomainManager.Mod.GetSetting(ModIdStr, "fullCombatAI", ref enable)
                && Combat.FullCombatAI.Enabled != enable
            )
            {
                Combat.FullCombatAI.Enabled = enable;
            }
        }
        {
            var enable = Combat.MissMe.Enabled;
            if (
                DomainManager.Mod.GetSetting(ModIdStr, "missMe", ref enable)
                && Combat.MissMe.Enabled != enable
            )
            {
                Combat.MissMe.Enabled = enable;
            }
        }
        {
            var enable = InitialSetup.AllGoodFeature.Enabled;
            if (
                DomainManager.Mod.GetSetting(ModIdStr, "goodFeature", ref enable)
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
                DomainManager.Mod.GetSetting(ModIdStr, "hobbyValue", ref enable)
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
            var enable = SkillBreakout.EndlessStep.Enabled;
            if (
                DomainManager.Mod.GetSetting(ModIdStr, "endlessStep", ref enable)
                && SkillBreakout.EndlessStep.Enabled != enable
            )
            {
                SkillBreakout.EndlessStep.Enabled = enable;
            }
        }
    }
}
