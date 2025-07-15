using System.Diagnostics;
using GameData.Utilities;

namespace LF2.Game.Helper;

internal sealed class DebugTraceListener : TraceListener
{
    public override void Write(string? message)
    {
        AdaptableLog.Info(message);
    }

    public override void WriteLine(string? message)
    {
        AdaptableLog.Info(message);
    }
}