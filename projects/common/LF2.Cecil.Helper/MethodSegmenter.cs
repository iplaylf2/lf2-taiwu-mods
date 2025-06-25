using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace LF2.Cecil.Helper;

public static class MethodSegmenter
{
    public abstract class LeftConfig<T>(MethodInfo prototype) where T : Delegate
    {
        internal protected readonly MethodInfo prototype = prototype;
        internal protected abstract IEnumerable<Type> InjectSplitPoint(ILCursor ilCursor);
    }

    public abstract class RightConfig<T>(
        MethodInfo prototype
    ) where T : Delegate
    {
        internal protected readonly MethodInfo prototype = prototype;
        internal protected abstract void InjectContinuationPoint(ILCursor ilCursor);
    }

    public static T CreateLeftSegment<T>(LeftConfig<T> config) where T : Delegate
    {
        InitILContext<T>(config.prototype, out var dynamicMethod, out var ilContext);

        ilContext.Invoke(ilContext =>
        {
            GuardOriginalReturns(ilContext, config.prototype);
            InjectSplitPoint(ilContext, config.InjectSplitPoint);
        });


        return dynamicMethod.Generate().CreateDelegate<T>();
    }

    public static T CreateRightSegment<T>(RightConfig<T> config) where T : Delegate
    {
        InitILContext<T>(config.prototype, out var dynamicMethod, out var ilContext);

        ilContext.Invoke(ilContext =>
        {
            var label = InjectContinuationPoint(ilContext, config.InjectContinuationPoint);
            RestoreExecutionContext(ilContext, label);
        });

        return dynamicMethod.Generate().CreateDelegate<T>();
    }

    private static void InitILContext<T>(
        MethodBase prototype,
        out DynamicMethodDefinition dynamicMethod,
        out ILContext ilContext
    ) where T : Delegate
    {
        var delegateType = typeof(T).GetMethod("Invoke");
        dynamicMethod = DynamicMethodDefinitionHelper.CreateFrom(
             prototype,
             delegateType.ReturnType,
             [.. delegateType.GetParameters().Select(x => x.ParameterType)]
         );
        ilContext = new ILContext(dynamicMethod.Definition);
    }

    public static void GuardOriginalReturns(ILContext ilContext, MethodInfo prototype)
    {
        var ilCursor = new ILCursor(ilContext);

        ilCursor.FindNext(out var retCursors, (x) => x.MatchRet());

        if (prototype.ReturnType == typeof(void))
        {
            PatchVoidReturns(retCursors);
        }
        else if (prototype.ReturnType.IsValueType)
        {
            PatchValueReturns(retCursors, prototype.ReturnType);
        }
        else
        {
            PatchObjectReturns(retCursors);
        }
    }

    public static void InjectSplitPoint(ILContext ilContext, Func<ILCursor, IEnumerable<Type>> injectSplitPoint)
    {
        var ilCursor = new ILCursor(ilContext);
        var stackValueTypes = injectSplitPoint(ilCursor);
        var statePacking = CreateStatePacking(stackValueTypes);

        ilCursor.Emit(OpCodes.Ldc_I4_1);

        CaptureLocals(ilCursor);

        ilCursor.Emit(OpCodes.Call, statePacking);

        ilCursor.Emit(OpCodes.Ret);
    }

    public static ILLabel InjectContinuationPoint(ILContext ilContext, Action<ILCursor> injectContinuationPoint)
    {
        var continuationLabel = ilContext.DefineLabel();
        var ilCursor = new ILCursor(ilContext);

        ilCursor.Emit(OpCodes.Br, continuationLabel);

        injectContinuationPoint(ilCursor);

        ilCursor.MarkLabel(continuationLabel);

        return continuationLabel;
    }

    public static void RestoreExecutionContext(ILContext ilContext, ILLabel continuationLabel)
    {
        var ilCursor = new ILCursor(ilContext);

        ilCursor.GotoLabel(continuationLabel);

        var stateIndex = ilContext.Method.Parameters.Count - 1;

        foreach (var (variable, i) in ilContext.Body.Variables.Select((x, i) => (x, i)))
        {
            ilCursor.Emit(OpCodes.Ldarg, stateIndex);
            ilCursor.Emit(OpCodes.Ldc_I4, i);
            ilCursor.Emit(OpCodes.Ldelem_Ref);

            if (variable.VariableType.IsValueType)
            {
                ilCursor.Emit(OpCodes.Unbox_Any, variable.VariableType);
            }
            else
            {
                ilCursor.Emit(OpCodes.Castclass, variable.VariableType);
            }

            ilCursor.Emit(OpCodes.Stloc, variable);
        }
    }

    private static MethodInfo CreateStatePacking(IEnumerable<Type> preservedStackTypes)
    {
        var stackValueParams = preservedStackTypes.Select((x) => Expression.Parameter(x)).ToArray();
        var isSplitReturnParam = Expression.Parameter(typeof(bool));
        var variablesParam = Expression.Parameter(typeof(object[]));
        ParameterExpression[] parameters = [.. stackValueParams, isSplitReturnParam, variablesParam];

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
                    isSplitReturnParam,
                    variablesParam
                ),
                parameters
            );

        return ExpressionHelper.CreateStaticMethod(lambda);
    }

    private static void PatchVoidReturns(ILCursor[] ilCursors)
    {
        foreach (var ilCursor in ilCursors)
        {
            ilCursor.Emit(OpCodes.Ldnull);
            ilCursor.EmitDelegate(WrapReturnValue);
        }
    }

    private static void PatchValueReturns(ILCursor[] ilCursors, Type type)
    {
        foreach (var ilCursor in ilCursors)
        {
            ilCursor.Emit(OpCodes.Box, type);
            ilCursor.EmitDelegate(WrapReturnValue);
        }
    }

    private static void PatchObjectReturns(ILCursor[] ilCursors)
    {
        foreach (var ilCursor in ilCursors)
        {
            ilCursor.EmitDelegate(WrapReturnValue);
        }
    }

    private static Tuple<object[], bool, object[]> WrapReturnValue(object returnValue)
    {
        return new([returnValue], false, []);
    }

    private static void CaptureLocals(ILCursor ilCursor)
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