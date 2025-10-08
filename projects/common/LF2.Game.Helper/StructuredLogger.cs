using System.Runtime.CompilerServices;
using GameData.Utilities;
using Newtonsoft.Json;

namespace LF2.Game.Helper;

public static class StructuredLogger
{
    public static void Error
    (
        string message,
        object? data = null,
        [CallerMemberName] string callerMember = ""
    )
    {
        AdaptableLog.Error(Format(message, data, callerMember));
    }

    public static void Info
    (
        string message,
        object? data = null,
        [CallerMemberName] string callerMember = ""
    )
    {
        AdaptableLog.Info(Format(message, data, callerMember));
    }

    public static void Warning
    (
        string message,
        object? data = null,
        [CallerMemberName] string callerMember = ""
    )
    {
        AdaptableLog.Warning(Format(message, data, callerMember));
    }

    private static string Format(string message, object? data, string callerMember)
    {
        var jsonString = JsonConvert.SerializeObject
        (
            new { callerMember, data, message },
            Formatting.Indented
        );

        return "\n" + jsonString;
    }
}