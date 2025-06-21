using System.Reflection;
using MonoMod.Cil;
using MonoMod.Utils;

namespace LF2.Cecil.Helper;

public static class DynamicMethodDefinitionHelper
{
    public static Type[] ExtraParameters(MethodBase origin)
    {
        return [
            ..origin.IsStatic?[origin.GetThisParamType()]:(Type[])[],
            ..origin.GetParameters().Select(x=>x.ParameterType)
        ];
    }

    public static DynamicMethodDefinition CreateFrom(
        MethodBase prototype,
        string name,
        Type returnType,
        Type[] parameterTypes
    )
    {
        var prototypeDMD = new DynamicMethodDefinition(prototype);
        var prototypeContext = new ILContext(prototypeDMD.Definition);

        var result = new DynamicMethodDefinition(
            name,
            returnType,
            parameterTypes
        );

        result.Definition.Body = prototypeContext.Body.Clone(result.Definition);
        result.OwnerType = prototypeDMD.OwnerType;

        return result;
    }
}