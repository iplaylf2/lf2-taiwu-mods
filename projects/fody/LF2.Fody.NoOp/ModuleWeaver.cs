using Fody;

namespace LF2.Fody.NoOp;

/// <summary>
/// A minimal weaver that proves the shared Fody infrastructure works end-to-end.
/// </summary>
public sealed class ModuleWeaver : BaseModuleWeaver
{
    public override void Execute()
    {
        WriteInfo("LF2.Fody.NoOp weaving - no IL modifications will be applied.");
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        return [];
    }
}
