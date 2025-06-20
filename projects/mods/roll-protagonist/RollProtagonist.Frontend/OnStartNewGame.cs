using HarmonyLib;
using UnityEngine;
using GameData.Utilities;
using System.Collections;
using MonoMod.Cil;
using System.Reflection;
using MonoMod.Utils;
using GameData.Domains.Character;
using System.Linq.Expressions;
using Mono.Cecil.Cil;
using GameData.Domains.Character.Creation;

namespace RollProtagonist.Frontend;

[HarmonyPatch(typeof(UI_NewGame), "DoStartNewGame")]
internal static class OnStartNewGamePatcher
{
    [HarmonyILManipulator]
    private static void SplitMethodIntoStages(MethodBase origin)
    {
        var stageA = new DynamicMethodDefinition(origin);

        {
            var ilContext = new ILContext(stageA.Definition);
            var ilCursor = new ILCursor(ilContext);
            var variables = ilContext.Body.Variables;

            ilContext.Method.ReturnType = ilContext.Module.ImportReference(typeof(Tuple<object[], bool, object[]>));

            var createProtagonist = CharacterDomainHelper.MethodCall.CreateProtagonist;
            var createProtagonistMethod = createProtagonist.GetMethodInfo();
            Type[] stackValueTypes = [typeof(int), typeof(ProtagonistCreationInfo)];
            var packResult = CreatePackResult(stackValueTypes);


            static void EmitPackLocals(ILCursor iLCursor, IEnumerable<VariableDefinition> variables)
            {
                iLCursor.Emit(OpCodes.Ldc_I4, variables.Count());
                iLCursor.Emit(OpCodes.Newarr, typeof(object));

                foreach (var (variable, index) in variables.Select((x, i) => (x, i)))
                {
                    iLCursor.Emit(OpCodes.Dup);
                    iLCursor.Emit(OpCodes.Ldc_I4, index);

                    iLCursor.Emit(OpCodes.Ldloc, variable);
                    if (variable.VariableType.IsValueType)
                    {
                        iLCursor.Emit(OpCodes.Box, variable.VariableType);
                    }

                    iLCursor.Emit(OpCodes.Stelem_Ref);
                }
            }

            {
                ilCursor.FindNext(out var targetCursors, (x) => x.MatchCallOrCallvirt(createProtagonistMethod));

                foreach (var targetCursor in targetCursors)
                {
                    targetCursor.Remove();
                    targetCursor.Emit(OpCodes.Ldc_I4_1);

                    EmitPackLocals(targetCursor, variables);

                    targetCursor.EmitDelegate(packResult);
                    targetCursor.Emit(OpCodes.Ret);
                }
            }

            {
                ilCursor.FindNext(out var retCursors, (x) => x.MatchRet());

                foreach (var retCursor in retCursors)
                {
                    retCursor.Index--;

                    retCursor.Emit(OpCodes.Ldc_I4_0);
                    retCursor.Emit(OpCodes.Ldnull);

                    retCursor.Emit(OpCodes.Ldc_I4_0);

                    EmitPackLocals(retCursor, variables);

                    retCursor.EmitDelegate(packResult);
                }
            }
        }

        BeforeRoll = stageA.Generate().CreateDelegate<Func<Tuple<object[], bool, object[]>>>();
    }

    [HarmonyPrefix]
    private static bool DoStartNewGamePrefix(UI_NewGame __instance)
    {
        __instance.StartCoroutine(DoStartNewGame(__instance));

        return false;
    }

    private static Delegate CreatePackResult(IEnumerable<Type> stackValues)
    {
        var stackValueParams = stackValues.Select((x, i) => Expression.Parameter(x, $"stackValue{i}")).ToArray();
        var isSplitParam = Expression.Parameter(typeof(bool), "isSplit");
        var variablesParam = Expression.Parameter(typeof(object[]), "variables");
        ParameterExpression[] parameters = [.. stackValueParams, isSplitParam, variablesParam];

        var objectType = typeof(object);

        return Expression
            .Lambda(
                Expression.New(
                    typeof(Tuple<object[], bool, object[]>).GetConstructors().First(),
                    Expression.NewArrayInit(
                        typeof(object),
                        stackValueParams.Select(
                            x => x.Type.IsValueType ? Expression.Convert(x, objectType) : (Expression)x
                        )
                    ),
                    isSplitParam,
                    variablesParam
                )
            )
            .Compile();
    }

    private static IEnumerator DoStartNewGame(UI_NewGame uiNewGame)
    {

        AdaptableLog.Info("before DoStartNewGame");

        // DoStartNewGameOrigin(uiNewGame);

        AdaptableLog.Info("after DoStartNewGame");

        yield return new WaitForSeconds(3);

        AdaptableLog.Info("after WaitForSeconds");
    }

    private static Func<Tuple<object[], bool, object[]>>? BeforeRoll;
}