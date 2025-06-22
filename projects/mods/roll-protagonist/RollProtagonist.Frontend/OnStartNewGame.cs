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
using LF2.Cecil.Helper;

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
        AdaptableLog.Info("SplitMethodIntoStages started");

        var createProtagonist = CharacterDomainHelper.MethodCall.CreateProtagonist;
        var createProtagonistMethod = createProtagonist.GetMethodInfo();

        BeforeRoll = MethodSplitter<BeforeRollDelegate>
            .CreateLeftSegment(origin)
            .CaptureLeftState(
                [typeof(int), typeof(ProtagonistCreationInfo)],
                (_, ilCursor) =>
                {
                    ilCursor.Emit(OpCodes.Ldc_I4_0);
                    ilCursor.Emit(OpCodes.Ldnull);
                }
            )
            .LeaveLeft((_, ilCursor) =>
            {
                ilCursor.GotoNext((x) => x.MatchCallOrCallvirt(createProtagonistMethod));
                ilCursor.Remove();
            })
            .CreateDelegate();

        AdaptableLog.Info("BeforeRoll generated");

        AfterRoll = MethodSplitter<AfterRollDelegate>
            .CreateRightSegment(origin)
            .EnterRight((_, ilCursor) =>
            {
                ilCursor.GotoNext((x) => x.MatchCallOrCallvirt(createProtagonistMethod));
                ilCursor.Index++;
            })
            .RestoreRightState()
            .CreateDelegate();

        AdaptableLog.Info("AfterRoll generated");
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
    private delegate Tuple<object[], bool, object[]> BeforeRollDelegate(UI_NewGame instance);
    private delegate void AfterRollDelegate(UI_NewGame instance, object[] variables);
    private static BeforeRollDelegate? BeforeRoll;
    private static AfterRollDelegate? AfterRoll;
}

class ILHelper
{
    public static MethodInfo CreateDynamicMethod(LambdaExpression lambda)
    {
        var targetDelegate = lambda.Compile();
        var delegateType = targetDelegate.GetType();
        var paramTypes = targetDelegate.Method
            .GetParameters()
            .Skip(1)
            .Select(p => p.ParameterType)
            .ToArray();

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