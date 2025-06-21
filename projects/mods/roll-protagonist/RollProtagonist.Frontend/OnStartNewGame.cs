using HarmonyLib;
using GameData.Utilities;
using System.Collections;
using MonoMod.Cil;
using System.Reflection;
using MonoMod.Utils;
using GameData.Domains.Character;
using System.Linq.Expressions;
using Mono.Cecil.Cil;
using GameData.Domains.Character.Creation;
using Mono.Cecil;

namespace RollProtagonist.Frontend;

[HarmonyPatch(typeof(UI_NewGame), "DoStartNewGame")]
internal static class OnStartNewGamePatcher
{
    private static IEnumerator DoStartNewGame(UI_NewGame uiNewGame)
    {
        var (stackValues, isRoll, variables) = BeforeRoll!(uiNewGame);

        if (!isRoll)
        {
            throw new InvalidOperationException("New game initialization failed.");
        }

        AdaptableLog.Info("Before roll");

        yield return null;

        CharacterDomainHelper.MethodCall.CreateProtagonist(
            (int)stackValues[0],
            (ProtagonistCreationInfo)stackValues[1]
        );

        yield return null;

        AdaptableLog.Info("After roll completed successfully");

        AfterRoll!(uiNewGame, variables);
    }

    [HarmonyILManipulator]
    private static void SplitMethodIntoStages(MethodBase origin)
    {
        var createProtagonist = CharacterDomainHelper.MethodCall.CreateProtagonist;
        var createProtagonistMethod = createProtagonist.GetMethodInfo();

        {
            var stage = new DynamicMethodDefinition(origin);

            var ilContext = new ILContext(stage.Definition);
            var ilCursor = new ILCursor(ilContext);

            ilContext.Method.ReturnType = ilContext.Module.ImportReference(typeof(Tuple<object[], bool, object[]>));

            var variables = ilContext.Body.Variables;
            Type[] stackValueTypes = [typeof(int), typeof(ProtagonistCreationInfo)];
            var packResult = CreatePackResult(stackValueTypes);

            static void EmitPackLocals(ILCursor iLCursor, ICollection<VariableDefinition> variables)
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

            {
                ilCursor.Index = 0;

                ilCursor.FindNext(out var retCursors, (x) => x.MatchRet());

                foreach (var retCursor in retCursors)
                {
                    retCursor.Index--;

                    retCursor.Emit(OpCodes.Ldc_I4_0);
                    retCursor.Emit(OpCodes.Ldnull);

                    retCursor.Emit(OpCodes.Ldc_I4_0);  // false

                    EmitPackLocals(retCursor, variables);

                    retCursor.Emit(OpCodes.Call, packResult);
                }
            }

            {
                ilCursor.Index = 0;

                var targetCursor = ilCursor.GotoNext((x) => x.MatchCallOrCallvirt(createProtagonistMethod));

                targetCursor.Remove();

                targetCursor.Emit(OpCodes.Ldc_I4_1); // true

                EmitPackLocals(targetCursor, variables);

                targetCursor.Emit(OpCodes.Call, packResult);

                targetCursor.Emit(OpCodes.Ret);
            }

            BeforeRoll = stage.Generate().CreateDelegate<Func<UI_NewGame, Tuple<object[], bool, object[]>>>();
        }

        // custom CreateProtagonist

        {
            var stage = new DynamicMethodDefinition(origin);

            var ilContext = new ILContext(stage.Definition);
            var ilCursor = new ILCursor(ilContext);

            var objectArrayType = ilContext.Module.ImportReference(typeof(object[]));
            ilContext.Method.Parameters.Add(new ParameterDefinition(objectArrayType));

            var variables = ilContext.Body.Variables;

            {
                var skipBefore = ilCursor.DefineLabel();

                ilCursor.Index = 0;

                ilCursor.Emit(OpCodes.Jmp, skipBefore);

                var targetCursor = ilCursor.GotoNext((x) => x.MatchCallOrCallvirt(createProtagonistMethod));

                ilCursor.MarkLabel(skipBefore);

                foreach (var (variable, i) in variables.Select((x, i) => (x, i)))
                {
                    ilCursor.Emit(OpCodes.Ldarg_1);
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

            AfterRoll = stage.Generate().CreateDelegate<Action<UI_NewGame, object[]>>();
        }
    }

    [HarmonyPrefix]
    private static bool DoStartNewGamePrefix(UI_NewGame __instance)
    {
        __instance.StartCoroutine(DoStartNewGame(__instance));

        return false;
    }

    private static MethodInfo CreatePackResult(IEnumerable<Type> stackValues)
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

        return ILHelper.CreateDynamicMethod(lambda);
    }

    private static Func<UI_NewGame, Tuple<object[], bool, object[]>>? BeforeRoll;
    private static Action<UI_NewGame, object[]>? AfterRoll;
}

class ILHelper
{
    public static MethodInfo CreateDynamicMethod(LambdaExpression lambda)
    {
        var targetDelegate = lambda.Compile();
        var delegateType = targetDelegate.GetType();
        var paramTypes = targetDelegate.Method.GetParameters().Select(p => p.ParameterType).ToArray();

        var dynamicMethod = new DynamicMethodDefinition(
            name: targetDelegate.Method.Name,
            returnType: targetDelegate.Method.ReturnType,
            parameterTypes: paramTypes
        );

        var il = dynamicMethod.GetILProcessor();
        var targetType = targetDelegate.Target.GetType();

        bool preserveContext = targetDelegate.Target != null &&
                              targetType.GetFields().Any(f => !f.IsStatic);

        if (preserveContext)
        {
            var currentDelegateCounter = DelegateCounter++;

            DelegateCache[currentDelegateCounter] = targetDelegate;

            var cacheField = AccessTools.Field(typeof(ILHelper), nameof(DelegateCache));
            var getMethod = AccessTools.Method(typeof(Dictionary<int, Delegate>), "get_Item");

            il.Emit(OpCodes.Ldsfld, cacheField);
            il.Emit(OpCodes.Ldc_I4, currentDelegateCounter);
            il.Emit(OpCodes.Callvirt, getMethod);
        }
        else
        {
            if (targetDelegate.Target == null)
            {
                il.Emit(OpCodes.Ldnull);
            }
            else
            {
                il.Emit(
                    OpCodes.Newobj,
                    AccessTools.FirstConstructor(targetType, x => x.GetParameters().Length == 0 && !x.IsStatic)
                );
            }

            il.Emit(OpCodes.Ldftn, targetDelegate.Method);
            il.Emit(OpCodes.Newobj, AccessTools.Constructor(delegateType, [typeof(object), typeof(IntPtr)]));
        }

        for (int i = 0; i < paramTypes.Length; i++)
        {
            il.Emit(OpCodes.Ldarg, i);
        }

        il.Emit(OpCodes.Callvirt, AccessTools.Method(delegateType, "Invoke"));
        il.Emit(OpCodes.Ret);

        return dynamicMethod.Generate();
    }

    private static readonly Dictionary<int, Delegate> DelegateCache = [];
    private static int DelegateCounter;
}