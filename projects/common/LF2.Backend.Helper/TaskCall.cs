using GameData.Common;
using GameData.Domains;
using GameData.Domains.Mod;
using LF2.Game.Helper;

namespace LF2.Backend.Helper;

public static class TaskCall
{
    public static void AddModMethod(
        string modIdStr,
        string methodName,
        Func<DataContext, SerializableModData, SerializableModData> method
    )
    {
        DomainManager.Mod.AddModMethod(
            modIdStr,
            methodName,
            (context, parameter) =>
            {
                var result = method(context, parameter);

                parameter.Get(CommonModConstants.CallIdKey, out int callId);
                result.Set(CommonModConstants.CallIdKey, callId);

                return result;
            }
        );
    }
}
