using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace LF2.Cecil.Helper;

[System.Diagnostics.CodeAnalysis.SuppressMessage
("Design", "CA1034:Nested types should not be visible", Justification = "<Pending>")
]
public static class MethodSegmenter
{
    public abstract class LeftConfig<T>(MethodInfo prototype) where T : Delegate
    {
        protected internal MethodInfo Prototype => prototype;
        protected internal abstract IEnumerable<Type> InjectSplitPoint(ILCursor ilCursor);
    }

    public abstract class RightConfig<T>
    (
        MethodInfo prototype
    ) where T : Delegate
    {
        protected internal MethodInfo Prototype => prototype;
        protected internal abstract void InjectContinuationPoint(ILCursor ilCursor);
    }

    public static T CreateLeftSegment<T>(LeftConfig<T> config) where T : Delegate
    {
        InitILContext<T>(config.Prototype, out var dynamicMethod, out var ilContext);

        using (dynamicMethod)
        using (ilContext)
        {
            ilContext.Invoke
            (
                ilContext =>
                {
                    GuardOriginalReturns(ilContext, config.Prototype);
                    InjectSplitPoint(ilContext, config.InjectSplitPoint);
                }
            );

            return dynamicMethod.Generate().CreateDelegate<T>();
        }
    }

    public static T CreateRightSegment<T>(RightConfig<T> config) where T : Delegate
    {
        InitILContext<T>(config.Prototype, out var dynamicMethod, out var ilContext);

        using (dynamicMethod)
        using (ilContext)
        {
            ilContext.Invoke
            (
                ilContext =>
                {
                    var label = InjectContinuationPoint(ilContext, config.InjectContinuationPoint);
                    RestoreExecutionContext(ilContext, label);
                }
            );

            return dynamicMethod.Generate().CreateDelegate<T>();
        }
    }

    private static void InitILContext<T>
    (
        MethodBase prototype,
        out DynamicMethodDefinition dynamicMethod,
        out ILContext ilContext
    ) where T : Delegate
    {
        var delegateType = typeof(T).GetMethod("Invoke")!;
        dynamicMethod = DynamicMethodDefinitionHelper.CreateFrom
        (
            prototype,
            delegateType.ReturnType,
            [.. delegateType.GetParameters().Select(x => x.ParameterType)]
        );
        ilContext = new ILContext(dynamicMethod.Definition);
    }

    private static void GuardOriginalReturns(ILContext ilContext, MethodInfo prototype)
    {
        var ilCursor = new ILCursor(ilContext);

        ilCursor.FindNext(out var retCursors, (x) => x.MatchRet());

        Action @do = prototype.ReturnType switch
        {
            var x when x == typeof(void) => () => PatchVoidReturns(retCursors),
            { IsValueType: true } => () => PatchValueReturns(retCursors, prototype.ReturnType),
            _ => () => PatchObjectReturns(retCursors)
        };

        @do();
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
        ParameterExpression[] parameters = [.. stackValueParams, isSplitReturnParam, variablesParam];

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