using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace LF2.Cecil.Helper;

public sealed class MethodSegmenter<T> :
    MethodSegmenter<T>.ISegmentMeta,
    MethodSegmenter<T>.IReturnGuard,
    MethodSegmenter<T>.ISplitPointInjector,
    MethodSegmenter<T>.IContinuationInjector,
    MethodSegmenter<T>.IContextRestorer,
    MethodSegmenter<T>.IDelegateBinder
    where T : Delegate
{
    public static IReturnGuard CreateLeftSegment(MethodBase prototype)
    {
        return new MethodSegmenter<T>(prototype);
    }

    public static IContinuationInjector CreateRightSegment(MethodBase prototype)
    {
        return new MethodSegmenter<T>(prototype);
    }

    public ISplitPointInjector GuardOriginalReturns()
    {
        var shouldBox = prototype.ReturnType.IsValueType;

        segmenterIlContext.Invoke(ilContext =>
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
        });

        return this;
    }

    public IDelegateBinder InjectSplitPoint(IEnumerable<Type> stackValueTypes, Action<ISegmentMeta, ILCursor> injectSplitPoint)
    {
        var statePacking = CreateStatePacking(stackValueTypes);

        segmenterIlContext.Invoke(ilContext =>
        {
            var ilCursor = new ILCursor(ilContext);

            injectSplitPoint(this, ilCursor);

            ilCursor.Emit(OpCodes.Ldc_I4_1);

            CaptureLocals(ilCursor);

            ilCursor.Emit(OpCodes.Call, statePacking);

            ilCursor.Emit(OpCodes.Ret);
        });

        return this;
    }

    public IContextRestorer InjectContinuationPoint(Action<ISegmentMeta, ILCursor> injectContinuationPoint)
    {
        segmenterIlContext.Invoke(ilContext =>
        {
            continuationLabel = ilContext.DefineLabel();

            var ilCursor = new ILCursor(ilContext);

            ilCursor.Emit(OpCodes.Br, continuationLabel);

            injectContinuationPoint(this, ilCursor);

            ilCursor.MarkLabel(continuationLabel);
        });

        return this;
    }

    public IDelegateBinder RestoreExecutionContext()
    {
        segmenterIlContext.Invoke(ilContext =>
        {
            var ilCursor = new ILCursor(ilContext);

            ilCursor.GotoLabel(continuationLabel!);

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
        });

        return this;
    }

    public T CreateDelegate()
    {
        return dynamicMethod.Generate().CreateDelegate<T>(null);
    }

    public interface ISegmentMeta
    {
        public MethodInfo DelegateType { get; }
    }

    public interface IReturnGuard
    {
        ISplitPointInjector GuardOriginalReturns();
    }

    public interface ISplitPointInjector
    {
        IDelegateBinder InjectSplitPoint(IEnumerable<Type> preservedStackTypes, Action<ISegmentMeta, ILCursor> injectSplitPoint);
    }

    public interface IContinuationInjector
    {
        IContextRestorer InjectContinuationPoint(Action<ISegmentMeta, ILCursor> injectContinuationPoint);
    }

    public interface IContextRestorer
    {
        IDelegateBinder RestoreExecutionContext();
    }

    public interface IDelegateBinder
    {
        T CreateDelegate();
    }

    public MethodInfo DelegateType { get; }

    private MethodSegmenter(MethodBase prototype)
    {
        this.prototype = (MethodInfo)prototype;

        DelegateType = typeof(T).GetMethod("Invoke");
        dynamicMethod = DynamicMethodDefinitionHelper.CreateFrom(
            prototype,
            DelegateType.ReturnType,
            [.. DelegateType.GetParameters().Select(x => x.ParameterType)]
        );
        segmenterIlContext = new ILContext(dynamicMethod.Definition);
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
    private readonly MethodInfo prototype;
    private readonly DynamicMethodDefinition dynamicMethod;
    private readonly ILContext segmenterIlContext;
    private ILLabel? continuationLabel;
}