using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace LF2.Cecil.Helper;

public class ExpressionHelper
{
    public static MethodInfo CreateStaticMethod(LambdaExpression lambda)
    {
        var freshDelegate = lambda.Compile();
        var paramTypes = freshDelegate.Method
            .GetParameters()
            .Skip(1)
            .Select(p => p.ParameterType)
            .ToArray();

        var dynamicMethod = new DynamicMethodDefinition(
            freshDelegate.Method.Name,
            freshDelegate.Method.ReturnType,
            paramTypes
        );

        var targetType = freshDelegate.Target.GetType();
        var il = dynamicMethod.GetILProcessor();
        var delegateType = freshDelegate.GetType();

        if (freshDelegate.Target != null
            && targetType.GetFields().Any(f => !f.IsStatic))
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
            if (freshDelegate.Target == null)
            {
                il.Emit(OpCodes.Ldnull);
            }
            else
            {
                il.Emit(
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