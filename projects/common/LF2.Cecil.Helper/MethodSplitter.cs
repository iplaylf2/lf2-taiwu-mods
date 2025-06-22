using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;

namespace LF2.Cecil.Helper;

public sealed class MethodSplitter<T> :
    MethodSplitter<T>.ISplitContext,
    MethodSplitter<T>.ILeftProtectOrigin,
    MethodSplitter<T>.ILeftLeave,
    MethodSplitter<T>.IRightEnter,
    MethodSplitter<T>.IRightRestore,
    MethodSplitter<T>.ICreateDelegate
    where T : Delegate
{
    public static ILeftProtectOrigin CreateLeftSegment(MethodBase prototype)
    {
        return new MethodSplitter<T>(prototype);
    }

    public static IRightEnter CreateRightSegment(MethodBase prototype)
    {
        return new MethodSplitter<T>(prototype);
    }

    public ILeftLeave ProtectLeftOrigin()
    {
        var shouldBox = prototype.ReturnType.IsValueType;

        splitContext.Invoke(ilContext =>
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

    public ICreateDelegate LeaveLeft(IEnumerable<Type> stackValueTypes, Action<ISplitContext, ILCursor> handleLeavePoint)
    {
        var statePack = CreateStatePack(stackValueTypes);

        splitContext.Invoke(ilContext =>
        {
            var ilCursor = new ILCursor(ilContext);

            handleLeavePoint(this, ilCursor);

            ilCursor.Emit(OpCodes.Ldc_I4_1);

            EmitLocalsPack(ilCursor);

            ilCursor.Emit(OpCodes.Call, statePack);

            ilCursor.Emit(OpCodes.Ret);
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

    public interface ILeftProtectOrigin
    {
        ILeftLeave ProtectLeftOrigin();
    }

    public interface ILeftLeave
    {
        ICreateDelegate LeaveLeft(IEnumerable<Type> stackValueTypes, Action<ISplitContext, ILCursor> handleLeftPoint);
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
        this.prototype = (MethodInfo)prototype;

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

    private static void PatchVoidReturns(ILCursor[] ilCursors)
    {
        foreach (var ilCursor in ilCursors)
        {
            ilCursor.Emit(OpCodes.Ldnull);
            ilCursor.EmitDelegate(PackOriginReturn);
        }
    }

    private static void PatchValueReturns(ILCursor[] ilCursors, Type type)
    {
        foreach (var ilCursor in ilCursors)
        {
            ilCursor.Emit(OpCodes.Box, type);
            ilCursor.EmitDelegate(PackOriginReturn);
        }
    }

    private static void PatchObjectReturns(ILCursor[] ilCursors)
    {
        foreach (var ilCursor in ilCursors)
        {
            ilCursor.EmitDelegate(PackOriginReturn);
        }
    }

    private static Tuple<object[], bool, object[]> PackOriginReturn(object returnValue)
    {
        return new([returnValue], false, []);
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
    private readonly MethodInfo prototype;
    private readonly DynamicMethodDefinition dynamicMethod;
    private readonly ILContext splitContext;
    private ILLabel? rightEntry;
}