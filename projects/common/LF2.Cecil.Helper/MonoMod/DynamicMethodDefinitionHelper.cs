using MonoMod.Utils;

namespace LF2.Cecil.Helper.MonoMod;

public static class DynamicMethodDefinitionHelper
{
    public static DynamicMethodDefinition CreateSkeleton<T>() where T : Delegate
    {
        var delegateType = typeof(T).GetMethod("Invoke")!;

        return new
        (
            delegateType.Name,
            delegateType.ReturnType,
            [.. delegateType.GetParameters().Select(x => x.ParameterType)]
        );
    }
}