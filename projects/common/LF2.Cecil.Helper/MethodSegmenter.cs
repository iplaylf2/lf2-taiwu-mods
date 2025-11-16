using HarmonyLib;
using LF2.Cecil.Helper.MonoMod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using System.Linq.Expressions;
using System.Reflection;

namespace LF2.Cecil.Helper;

public static class MethodSegmenter
{
    public static T CreateLeftSegment<T>
    (
        MethodInfo prototype,
        Func<ILCursor, IEnumerable<Type>> injectSplitPoint
    )
    where T : Delegate
    {
        using var sourceMethod = new DynamicMethodDefinition(prototype);
        using var ilContext = new ILContext(sourceMethod.Definition);

        ilContext.Invoke
        (
            ilContext =>
            {
                GuardOriginalReturns(ilContext, prototype.ReturnType);
                InjectSplitPoint(ilContext, injectSplitPoint);
            }
        );

        return CreateDelegate<T>(sourceMethod);
    }

    public static T CreateRightSegment<T>
    (
        MethodInfo prototype,
        Action<ILCursor> injectContinuationPoint
    )
    where T : Delegate
    {
        using var sourceMethod = new DynamicMethodDefinition(prototype);
        using var ilContext = new ILContext(sourceMethod.Definition);

        ilContext.Invoke
        (
            ilContext =>
            {
                var label = InjectContinuationPoint(ilContext, injectContinuationPoint);

                RestoreExecutionContext(ilContext, label);
            }
        );

        return CreateDelegate<T>(sourceMethod);
    }

    private static T CreateDelegate<T>(DynamicMethodDefinition sourceMethod) where T : Delegate
    {
        using var targetMethod = DynamicMethodDefinitionHelper.CreateSkeleton<T>();

        targetMethod.Definition.Body = sourceMethod.Definition.Body.Clone(targetMethod.Definition);
        targetMethod.OwnerType = sourceMethod.OwnerType;

        return targetMethod.Generate().CreateDelegate<T>();
    }

    private static void GuardOriginalReturns(ILContext ilContext, Type returnType)
    {
        var ilCursor = new ILCursor(ilContext);

        ilCursor.FindNext(out var retCursors, (x) => x.MatchRet());

        Action doPatch = returnType switch
        {
            var x when x == typeof(void) => () => PatchVoidReturns(retCursors),
            var x when x is { IsValueType: true } => () => PatchValueReturns(retCursors, x),
            _ => () => PatchObjectReturns(retCursors)
        };

        doPatch();
    }

    private static void InjectSplitPoint(ILContext ilContext, Func<ILCursor, IEnumerable<Type>> injectSplitPoint)
    {
        var ilCursor = new ILCursor(ilContext);
        var stackValueTypes = injectSplitPoint(ilCursor);
        var statePacking = CreateStatePacking(stackValueTypes);

        _ = ilCursor.Emit(OpCodes.Ldc_I4_1);

        CaptureLocals(ilCursor);

        _ = ilCursor
        .Emit(OpCodes.Call, statePacking)
        .Emit(OpCodes.Ret);
    }

    private static ILLabel InjectContinuationPoint(ILContext ilContext, Action<ILCursor> injectContinuationPoint)
    {
        var continuationLabel = ilContext.DefineLabel();
        var ilCursor = new ILCursor(ilContext);

        _ = ilCursor.Emit(OpCodes.Br, continuationLabel);

        injectContinuationPoint(ilCursor);
        ilCursor.MarkLabel(continuationLabel);

        return continuationLabel;
    }

    private static void RestoreExecutionContext(ILContext ilContext, ILLabel continuationLabel)
    {
        var ilCursor = new ILCursor(ilContext);

        _ = ilCursor.GotoLabel(continuationLabel);

        var stateIndex = ilContext.Method.Parameters.Count - 1;

        foreach (var (variable, i) in ilContext.Body.Variables.Select((x, i) => (x, i)))
        {
            _ = ilCursor
            .Emit(OpCodes.Ldarg, stateIndex)
            .Emit(OpCodes.Ldc_I4, i)
            .Emit(OpCodes.Ldelem_Ref);

            _ = variable.VariableType.IsValueType
            ? ilCursor.Emit(OpCodes.Unbox_Any, variable.VariableType)
            : ilCursor.Emit(OpCodes.Castclass, variable.VariableType);

            _ = ilCursor.Emit(OpCodes.Stloc, variable);
        }
    }

    private static MethodInfo CreateStatePacking(IEnumerable<Type> preservedStackTypes)
    {
        var stackValueParams = preservedStackTypes.Select(Expression.Parameter).ToArray();
        var isSplitReturnParam = Expression.Parameter(typeof(bool));
        var variablesParam = Expression.Parameter(typeof(object[]));
        var parameters = Array.AsReadOnly([.. stackValueParams, isSplitReturnParam, variablesParam]);
        var objectType = typeof(object);
        var lambda = Expression
        .Lambda
        (
            Expression.New
            (
                AccessTools.FirstConstructor
                (
                    typeof(Tuple<object[], bool, object[]>),
                    x => x.GetParameters().Length == 3
                ),
                Expression.NewArrayInit
                (
                    typeof(object),
                    stackValueParams.Select
                    (
                        x => x.Type.IsValueType ? Expression.Convert(x, objectType) : (Expression)x
                    )
                ),
                isSplitReturnParam,
                variablesParam
            ),
            parameters
        );

        return ExpressionHelper.ToStaticMethod(lambda);
    }

    private static void PatchVoidReturns(ILCursor[] ilCursors)
    {
        foreach (var ilCursor in ilCursors)
        {
            _ = ilCursor
            .Emit(OpCodes.Ldnull)
            .EmitDelegate(WrapReturnValue);
        }
    }

    private static void PatchValueReturns(ILCursor[] ilCursors, Type type)
    {
        foreach (var ilCursor in ilCursors)
        {
            _ = ilCursor
            .Emit(OpCodes.Box, type)
            .EmitDelegate(WrapReturnValue);
        }
    }

    private static void PatchObjectReturns(ILCursor[] ilCursors)
    {
        foreach (var ilCursor in ilCursors)
        {
            _ = ilCursor.EmitDelegate(WrapReturnValue);
        }
    }

    private static Tuple<object[], bool, object[]> WrapReturnValue(object returnValue)
    {
        return new([returnValue], false, []);
    }

    private static void CaptureLocals(ILCursor ilCursor)
    {
        var variables = ilCursor.Body.Variables;

        _ = ilCursor
        .Emit(OpCodes.Ldc_I4, variables.Count)
        .Emit(OpCodes.Newarr, typeof(object));

        foreach (var (variable, i) in variables.Select((x, i) => (x, i)))
        {
            _ = ilCursor
            .Emit(OpCodes.Dup)
            .Emit(OpCodes.Ldc_I4, i)
            .Emit(OpCodes.Ldloc, variable);

            if (variable.VariableType.IsValueType)
            {
                _ = ilCursor.Emit(OpCodes.Box, variable.VariableType);
            }

            _ = ilCursor.Emit(OpCodes.Stelem_Ref);
        }
    }
}
