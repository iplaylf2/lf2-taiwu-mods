using GameData.Domains;
using HarmonyLib;
using TaiwuModdingLib.Core.Plugin;

namespace TiredSL.Backend;

[PluginConfig("MyTaiwu", "lf2", "1.0.0")]
public class BackendExport : TaiwuRemakeHarmonyPlugin, IDisposable
{
    private Harmony? harmony;

    public override void Dispose()
    {
        harmony?.UnpatchSelf();
    }

    public override void Initialize()
    {
        harmony = new Harmony("lf2");

        harmony.PatchAll(typeof(Random.RandomImprove));

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
        DomainManager.Mod.GetSetting(ModIdStr, "niceCatch", ref Combat.CollapseCatchOdds.Enabled);
        DomainManager.Mod.GetSetting(ModIdStr, "fullCombatAI", ref Combat.FullCombatAI.Enabled);
        DomainManager.Mod.GetSetting(ModIdStr, "missMe", ref Combat.MissMe.Enabled);
        DomainManager.Mod.GetSetting(ModIdStr, "goodFeature", ref InitialSetup.AllGoodFeature.Enabled);
        DomainManager.Mod.GetSetting(ModIdStr, "canMoveResource", ref InitialSetup.CanMoveResource.Enabled);
        DomainManager.Mod.GetSetting(ModIdStr, "hobbyValue", ref InitialSetup.MyHobbyValue.Enabled);
        DomainManager.Mod.GetSetting(ModIdStr, "brightenUp", ref SkillBreakout.BrightenUp.Enabled);
        DomainManager.Mod.GetSetting(ModIdStr, "endlessStep", ref SkillBreakout.EndlessStep.Enabled);
    }
}
