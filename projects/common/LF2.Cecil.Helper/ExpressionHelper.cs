using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace LF2.Cecil.Helper;

public static class ExpressionHelper
{
    public static MethodInfo ToStaticMethod(LambdaExpression lambda)
    {
        var freshDelegate = lambda.Compile();
        var paramTypes = freshDelegate.Method
        .GetParameters()
        .Skip(1)
        .Select(p => p.ParameterType)
        .ToArray();

        using var dynamicMethod = new DynamicMethodDefinition
        (
            freshDelegate.Method.Name,
            freshDelegate.Method.ReturnType,
            paramTypes
        );

        var il = dynamicMethod.GetILProcessor();
        var delegateType = freshDelegate.GetType();
        var targetType = freshDelegate.Target?.GetType();

        if (targetType?.GetFields().Any(f => !f.IsStatic) ?? false)
        {
            var currentDelegateCounter = DelegateCounter++;

            DelegateCache[currentDelegateCounter] = freshDelegate;

            var cacheField = AccessTools.Field(typeof(ExpressionHelper), nameof(DelegateCache));
            var getMethod = AccessTools.Method(typeof(Dictionary<int, Delegate>), "get_Item");

            il.Emit(OpCodes.Ldsfld, cacheField);
            il.Emit(OpCodes.Ldc_I4, currentDelegateCounter);
            il.Emit(OpCodes.Callvirt, getMethod);
        }
        else
        {
            if (targetType == null)
            {
                il.Emit(OpCodes.Ldnull);
            }
            else
            {
                il.Emit
                (
                    OpCodes.Newobj,
                    AccessTools.FirstConstructor(targetType, x => x.GetParameters().Length == 0 && !x.IsStatic)
                );
            }

            il.Emit(OpCodes.Ldftn, freshDelegate.Method);
            il.Emit(OpCodes.Newobj, AccessTools.Constructor(delegateType, [typeof(object), typeof(IntPtr)]));
        }

        for (int i = 0; i < paramTypes.Length; i++)
        {
            il.Emit(OpCodes.Ldarg, i);
        }

        il.Emit(OpCodes.Callvirt, AccessTools.Method(delegateType, "Invoke"));
        il.Emit(OpCodes.Ret);

        return dynamicMethod.Generate();
    }

    private static readonly Dictionary<int, Delegate> DelegateCache = [];
    private static int DelegateCounter;
}