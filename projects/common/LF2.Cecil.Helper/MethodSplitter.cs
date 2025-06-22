using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace LF2.Cecil.Helper;

public sealed class MethodSplitter<T> :
    MethodSplitter<T>.ISplitContext,
    MethodSplitter<T>.IStateStorer,
    MethodSplitter<T>.IReturnPointHandler,
    MethodSplitter<T>.IBranchPointHandler,
    MethodSplitter<T>.IStateRestorer,
    MethodSplitter<T>.IDelegateFinalizer
    where T : Delegate
{
    public static MethodSplitter<T>.IStateStorer CreateLeftSplit(MethodBase prototype)
    {
        return new MethodSplitter<T>(prototype);
    }

    public static MethodSplitter<T>.IBranchPointHandler CreateRightSplit(MethodBase prototype)
    {
        return new MethodSplitter<T>(prototype);
    }

    public IReturnPointHandler StoreState(IEnumerable<Type> stackValues, Action<ISplitContext, ILCursor> aliasStack)
    {
        stateBundleMethod = CreateStateBundle(stackValues);

        splitContext.Invoke(ilContext =>
        {
            var ilCursor = new ILCursor(ilContext);

            PatchReturnPoints(ilCursor, ilCursor => aliasStack(this, ilCursor), stateBundleMethod);
        });

        return this;
    }

    public IDelegateFinalizer HandleReturn(Action<ISplitContext, ILCursor> handler)
    {
        splitContext.Invoke(ilContext =>
        {
            var ilCursor = new ILCursor(ilContext);

            handler(this, ilCursor);

            EmitStateBundleReturn(ilCursor, true, stateBundleMethod!);
        });

        return this;
    }

    public IStateRestorer HandleBranch(Action<ISplitContext, ILCursor> handler)
    {
        splitContext.Invoke(ilContext =>
        {
            branchPoint = ilContext.DefineLabel();
            var ilCursor = new ILCursor(ilContext);

            ilCursor.Emit(OpCodes.Br, branchPoint);

            handler(this, ilCursor);

            ilCursor.MarkLabel(branchPoint);
        });

        return this;
    }

    public IDelegateFinalizer RestoreState()
    {
        splitContext.Invoke(ilContext =>
        {
            var ilCursor = new ILCursor(ilContext);

            ilCursor.GotoLabel(branchPoint!);

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

    public interface IStateStorer
    {
        IReturnPointHandler StoreState(IEnumerable<Type> stackValues, Action<ISplitContext, ILCursor> aliasStack);
    }

    public interface IReturnPointHandler
    {
        IDelegateFinalizer HandleReturn(Action<ISplitContext, ILCursor> handler);
    }

    public interface IBranchPointHandler
    {
        IStateRestorer HandleBranch(Action<ISplitContext, ILCursor> handler);
    }

    public interface IStateRestorer
    {
        IDelegateFinalizer RestoreState();
    }

    public interface IDelegateFinalizer
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

    private static MethodInfo CreateStateBundle(IEnumerable<Type> stackValues)
    {
        var stackValueParams = stackValues.Select((x) => Expression.Parameter(x)).ToArray();
        var isSplitParam = Expression.Parameter(typeof(bool));
        var variablesParam = Expression.Parameter(typeof(object[]));
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

    private static void PatchReturnPoints(ILCursor ilCursor, Action<ILCursor> aliasStack, MethodInfo statePack)
    {
        ilCursor.FindNext(out var retCursors, (x) => x.MatchRet());

        foreach (var retCursor in retCursors)
        {
            retCursor.Remove();

            aliasStack(retCursor);

            EmitStateBundleReturn(ilCursor, false, statePack);
        }
    }

    private static void EmitStateBundleReturn(ILCursor ilCursor, bool isSplitPosition, MethodInfo statePack)
    {
        ilCursor.Emit(isSplitPosition ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);

        EmitLocalVariablesBundle(ilCursor);

        ilCursor.Emit(OpCodes.Call, statePack);

        ilCursor.Emit(OpCodes.Ret);
    }

    private static void EmitLocalVariablesBundle(ILCursor ilCursor)
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
    private MethodInfo? stateBundleMethod;
    private ILLabel? branchPoint;
}


