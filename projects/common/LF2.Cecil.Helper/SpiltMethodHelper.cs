using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace LF2.Cecil.Helper;

public static class SpiltMethodHelper
{
    private static MethodInfo CreateStatePack(IEnumerable<Type> stackValues)
    {
        var stackValueParams = stackValues.Select((x, i) => Expression.Parameter(x, $"stackValue{i}")).ToArray();
        var isSplitParam = Expression.Parameter(typeof(bool), "isSplit");
        var variablesParam = Expression.Parameter(typeof(object[]), "variables");
        ParameterExpression[] parameters = [.. stackValueParams, isSplitParam, variablesParam];

        var objectType = typeof(object);

        var lambda = Expression
            .Lambda(
                Expression.New(
                    AccessTools.FirstConstructor(
                        typeof(Tuple<object[], bool, object[]>),
                         x => x.GetParameters().Length == 3
                    ),
                    Expression.NewArrayInit(
                        typeof(object),
                        stackValueParams.Select(
                            x => x.Type.IsValueType ? Expression.Convert(x, objectType) : (Expression)x
                        )
                    ),
                    isSplitParam,
                    variablesParam
                ),
                parameters
            );

        return ExpressionHelper.CreateStaticMethod(lambda);
    }

    private static void AdaptReturn(ILCursor ilCursor, Action<ILCursor> aliasStack, MethodInfo statePack)
    {
        ilCursor.FindNext(out var retCursors, (x) => x.MatchRet());

        foreach (var retCursor in retCursors)
        {
            retCursor.Remove();

            aliasStack(retCursor);

            EmitNewReturn(ilCursor, false, statePack);
        }
    }

    private static void EmitNewReturn(ILCursor ilCursor, bool isTargetBranch, MethodInfo statePack)
    {
        ilCursor.Emit(isTargetBranch ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);

        EmitPackLocals(ilCursor);

        ilCursor.Emit(OpCodes.Call, statePack);

        ilCursor.Emit(OpCodes.Ret);
    }

    private static void EmitPackLocals(ILCursor ilCursor)
    {
        var variables = ilCursor.Body.Variables;

        ilCursor.Emit(OpCodes.Ldc_I4, variables.Count);
        ilCursor.Emit(OpCodes.Newarr, typeof(object));

        foreach (var (variable, i) in variables.Select((x, i) => (x, i)))
        {
            ilCursor.Emit(OpCodes.Dup);
            ilCursor.Emit(OpCodes.Ldc_I4, i);
            ilCursor.Emit(OpCodes.Ldloc, variable);

            if (variable.VariableType.IsValueType)
            {
                ilCursor.Emit(OpCodes.Box, variable.VariableType);
            }

            ilCursor.Emit(OpCodes.Stelem_Ref);
        }
    }
}
