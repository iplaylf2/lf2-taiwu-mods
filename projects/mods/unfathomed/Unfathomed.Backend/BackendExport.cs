using TaiwuModdingLib.Core.Plugin;

namespace Unfathomed.Backend;

[PluginConfig("unfathomed", "lf2", "1.0.0")]
public class BackendExport : TaiwuRemakeHarmonyPlugin, IDisposable
{
    public override void Initialize()
    {
        HarmonyInstance.PatchAll(typeof(AgeCompletion.BuildingDomainPatcher));
    }
}