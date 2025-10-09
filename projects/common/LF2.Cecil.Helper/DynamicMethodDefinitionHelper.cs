using System.Reflection;
using MonoMod.Cil;
using MonoMod.Utils;

namespace LF2.Cecil.Helper;

public static class DynamicMethodDefinitionHelper
{
    public static DynamicMethodDefinition CreateFrom
    (
        MethodBase prototype,
        Type returnType,
        Type[] parameterTypes
    )
    {
        using var prototypeDMD = new DynamicMethodDefinition(prototype);
        using var prototypeContext = new ILContext(prototypeDMD.Definition);

        var result = new DynamicMethodDefinition
        (
            prototypeDMD.Name,
            returnType,
            parameterTypes
        );

        result.Definition.Body = prototypeContext.Body.Clone(result.Definition);
        result.OwnerType = prototypeDMD.OwnerType;

        return result;
    }
}