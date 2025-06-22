using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace LF2.Cecil.Helper;

public sealed class MethodSplitter<T> :
    MethodSplitter<T>.ISplitContext,
    MethodSplitter<T>.ILeftCapture,
    MethodSplitter<T>.ILeftLeave,
    MethodSplitter<T>.IRightEnter,
    MethodSplitter<T>.IRightRestore,
    MethodSplitter<T>.ICreateDelegate
    where T : Delegate
{
    public static ILeftCapture CreateLeftSegment(MethodBase prototype)
    {
        return new MethodSplitter<T>(prototype);
    }

    public static IRightEnter CreateRightSegment(MethodBase prototype)
    {
        return new MethodSplitter<T>(prototype);
    }

    public ILeftLeave CaptureLeftState(IEnumerable<Type> stackValues, Action<ISplitContext, ILCursor> alignStack)
    {
        statePackM = CreateStatePack(stackValues);

        splitContext.Invoke(ilContext =>
        {
            var ilCursor = new ILCursor(ilContext);

            PatchOriginalReturns(ilCursor, ilCursor => alignStack(this, ilCursor), statePackM);
        });

        return this;
    }

    public ICreateDelegate LeaveLeft(Action<ISplitContext, ILCursor> handleLeavePoint)
    {
        splitContext.Invoke(ilContext =>
        {
            var ilCursor = new ILCursor(ilContext);

            handleLeavePoint(this, ilCursor);

            EmitPackReturn(ilCursor, true, statePackM!);
        });

        return this;
    }

    public IRightRestore EnterRight(Action<ISplitContext, ILCursor> handleEnterPoint)
    {
        splitContext.Invoke(ilContext =>
        {
            rightEntry = ilContext.DefineLabel();
            
            var ilCursor = new ILCursor(ilContext);

            ilCursor.Emit(OpCodes.Br, rightEntry);

            handleEnterPoint(this, ilCursor);

            ilCursor.MarkLabel(rightEntry);
        });

        return this;
    }

    public ICreateDelegate RestoreRightState()
    {
        splitContext.Invoke(ilContext =>
        {
            var ilCursor = new ILCursor(ilContext);

            ilCursor.GotoLabel(rightEntry!);

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

    public interface ISplitContext
    {
        public MethodInfo DelegateType { get; }
    }

    public interface ILeftCapture
    {
        ILeftLeave CaptureLeftState(IEnumerable<Type> stackValueTypes, Action<ISplitContext, ILCursor> alignStack);
    }

    public interface ILeftLeave
    {
        ICreateDelegate LeaveLeft(Action<ISplitContext, ILCursor> handleLeftPoint);
    }

    public interface IRightEnter
    {
        IRightRestore EnterRight(Action<ISplitContext, ILCursor> handleEnterPoint);
    }

    public interface IRightRestore
    {
        ICreateDelegate RestoreRightState();
    }

    public interface ICreateDelegate
    {
        T CreateDelegate();
    }

    public MethodInfo DelegateType { get; }

    private MethodSplitter(MethodBase prototype)
    {
        DelegateType = typeof(T).GetMethod("Invoke");
        dynamicMethod = DynamicMethodDefinitionHelper.CreateFrom(
            prototype,
            DelegateType.ReturnType,
            [.. DelegateType.GetParameters().Select(x => x.ParameterType)]
        );
        splitContext = new ILContext(dynamicMethod.Definition);
    }

    private static MethodInfo CreateStatePack(IEnumerable<Type> stackValueTypes)
    {
        var stackValueParams = stackValueTypes.Select((x) => Expression.Parameter(x)).ToArray();
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

    private static void PatchOriginalReturns(ILCursor ilCursor, Action<ILCursor> aliasStack, MethodInfo statePack)
    {
        ilCursor.FindNext(out var retCursors, (x) => x.MatchRet());

        foreach (var retCursor in retCursors)
        {
            retCursor.Remove();

            aliasStack(retCursor);

            EmitPackReturn(ilCursor, false, statePack);
        }
    }

    private static void EmitPackReturn(ILCursor ilCursor, bool isSplitReturn, MethodInfo statePack)
    {
        ilCursor.Emit(isSplitReturn ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);

        EmitLocalsPack(ilCursor);

        ilCursor.Emit(OpCodes.Call, statePack);

        ilCursor.Emit(OpCodes.Ret);
    }

    private static void EmitLocalsPack(ILCursor ilCursor)
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

    private readonly DynamicMethodDefinition dynamicMethod;
    private readonly ILContext splitContext;
    private MethodInfo? statePackM;
    private ILLabel? rightEntry;
}