using Cysharp.Threading.Tasks;
using TaiwuModdingLib.Core.Plugin;
using UnityEngine.LowLevel;

namespace UniTaskSupport.Frontend;

[PluginConfig("uni-task-support", "lf2", "1.0.0")]
public class FrontendExport : TaiwuRemakeHarmonyPlugin
{
    public override void Initialize()
    {
        if (PlayerLoopHelper.IsInjectedUniTaskPlayerLoop())
        {
            return;
        }

        var loop = PlayerLoop.GetCurrentPlayerLoop();

        PlayerLoopHelper.Initialize(ref loop);

        Game.ClockAndLogInfo("InitUniTaskLoop", false);
    }
}