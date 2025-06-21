using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace LF2.Cecil.Helper;

public static class SpiltMethodHelper
{
    public static void EmitPackLocals(ILCursor iLCursor, ICollection<VariableDefinition> variables)
    {
        iLCursor.Emit(OpCodes.Ldc_I4, variables.Count);
        iLCursor.Emit(OpCodes.Newarr, typeof(object));

        foreach (var (variable, i) in variables.Select((x, i) => (x, i)))
        {
            iLCursor.Emit(OpCodes.Dup);
            iLCursor.Emit(OpCodes.Ldc_I4, i);
            iLCursor.Emit(OpCodes.Ldloc, variable);

            if (variable.VariableType.IsValueType)
            {
                iLCursor.Emit(OpCodes.Box, variable.VariableType);
            }

            iLCursor.Emit(OpCodes.Stelem_Ref);
        }
    }

    public static MethodInfo CreateStatePack(IEnumerable<Type> stackValues)
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

    public static void AdaptReturn()
    {

    }
}
