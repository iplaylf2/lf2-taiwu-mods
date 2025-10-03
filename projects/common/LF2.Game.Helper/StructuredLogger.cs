using System.Runtime.CompilerServices;
using GameData.Utilities;
using Newtonsoft.Json;

namespace LF2.Game.Helper;

internal static class StructuredLogger
{
    public static void Info
    (
        string message,
        object? data = null,
        [CallerMemberName] string callerMember = ""
    )
    {
        var jsonString = JsonConvert.SerializeObject
        (
            new { callerMember, data, message },
            Formatting.Indented
        );

        AdaptableLog.Info("\n" + jsonString);
    }
}