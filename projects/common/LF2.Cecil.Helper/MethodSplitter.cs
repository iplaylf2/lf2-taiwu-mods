using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace LF2.Cecil.Helper;

public sealed class MethodSplitter<T> :
    MethodSplitter<T>.ISplitContext,
    MethodSplitter<T>.ILeftProcessState,
    MethodSplitter<T>.ILeftProcessCursor,
    MethodSplitter<T>.IGenerateDelegate
    where T : Delegate
{
    public static MethodSplitter<T>.ILeftProcessState SplitLeft(MethodBase prototype)
    {
        return new MethodSplitter<T>(prototype);
    }

    public ILeftProcessCursor ProcessMethodState(IEnumerable<Type> stackValues, Action<ISplitContext, ILCursor> aliasStack)
    {
        LeftStatePack = CreateStatePack(stackValues);

        SplitContext.Invoke(ilContext =>
        {
            var ilCursor = new ILCursor(ilContext);

            AdaptReturn(ilCursor, ilCursor => aliasStack(this, ilCursor), LeftStatePack);
        });

        return this;
    }

    public IGenerateDelegate ProcessSplitCursor(Action<ISplitContext, ILCursor> handleSplit)
    {
        SplitContext.Invoke(ilContext =>
        {
            var ilCursor = new ILCursor(ilContext);

            handleSplit(this, ilCursor);

            EmitLeftReturn(ilCursor, true, LeftStatePack!);
        });

        return this;
    }

    public T Generate()
    {
        return DMD.Generate().CreateDelegate<T>(null);
    }

    public interface ISplitContext
    {
        public MethodInfo TargetType { get; }
    }

    public interface ILeftProcessState
    {
        ILeftProcessCursor ProcessMethodState(IEnumerable<Type> stackValues, Action<ISplitContext, ILCursor> aliasStack);
    }

    public interface ILeftProcessCursor
    {
        IGenerateDelegate ProcessSplitCursor(Action<ISplitContext, ILCursor> handleSplit);
    }

    public interface IGenerateDelegate
    {
        T Generate();
    }

    public MethodInfo TargetType { get; }

    private MethodSplitter(MethodBase prototype)
    {
        TargetType = typeof(T).GetMethod("Invoke");
        DMD = DynamicMethodDefinitionHelper.CreateFrom(
            prototype,
            TargetType.ReturnType,
            [.. TargetType.GetParameters().Select(x => x.ParameterType)]
        );
        SplitContext = new ILContext(DMD.Definition);
    }

    private static MethodInfo CreateStatePack(IEnumerable<Type> stackValues)
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

    private static void AdaptReturn(ILCursor ilCursor, Action<ILCursor> aliasStack, MethodInfo statePack)
    {
        ilCursor.FindNext(out var retCursors, (x) => x.MatchRet());

        foreach (var retCursor in retCursors)
        {
            retCursor.Remove();

            aliasStack(retCursor);

            EmitLeftReturn(ilCursor, false, statePack);
        }
    }

    private static void EmitLeftReturn(ILCursor ilCursor, bool isSplitPosition, MethodInfo statePack)
    {
        ilCursor.Emit(isSplitPosition ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);

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

    private readonly DynamicMethodDefinition DMD;
    private readonly ILContext SplitContext;
    private MethodInfo? LeftStatePack;
}


