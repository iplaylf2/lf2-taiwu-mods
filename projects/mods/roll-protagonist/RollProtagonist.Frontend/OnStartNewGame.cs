using HarmonyLib;
using UnityEngine;
using GameData.Utilities;
using System.Collections;
using MonoMod.Cil;
using System.Reflection;
using MonoMod.Utils;
using GameData.Domains.Character;
using System.Linq.Expressions;

namespace RollProtagonist.Frontend;

[HarmonyPatch(typeof(UI_NewGame), "DoStartNewGame")]
internal static class OnStartNewGamePatcher
{
    [HarmonyILManipulator]
    private static void SplitMethodIntoStages(MethodBase origin)
    {
        var stageA = new DynamicMethodDefinition(origin);  // origin 是 void DoStartNewGame()

        {
            var ilContext = new ILContext(stageA.Definition);
            var ilCursor = new ILCursor(ilContext);
            var createProtagonist = CharacterDomainHelper.MethodCall.CreateProtagonist;
            var createProtagonistMethod = createProtagonist.GetMethodInfo();

            {


                ilCursor.FindNext(out var targets, (x) => x.MatchCallOrCallvirt(createProtagonistMethod));

                foreach (var target in targets)
                {
                    target.Remove();
                    // 插入指令： targetMethod 的两个参数在计算栈里，加上局部变量表，和true值表达是分割位置返回，整合后 return
                }
            }

            {
                ilCursor.FindNext(out var targets, (x) => x.MatchRet());

                foreach (var target in targets)
                {
                    target.Index--;
                    // 插入指令：计算栈是空的，填充一些与前面对的空白齐内容，和false值表达非分割位置返回
                }
            }

        }

        // BeforeRoll = stageA.Generate().CreateDelegate<>();
    }

    [HarmonyPrefix]
    private static bool DoStartNewGamePrefix(UI_NewGame __instance)
    {
        __instance.StartCoroutine(DoStartNewGame(__instance));

        return false;
    }

    private static Delegate CreatePackFunc(IEnumerable<Type> stackValues, IEnumerable<Type> variables)
    {
        var stackValueParams = stackValues.Select((x, i) => Expression.Parameter(x, $"stackValue{i}")).ToArray();
        var isSplitParam = Expression.Parameter(typeof(bool), "isSplit");
        var variableParams = variables.Select((x, i) => Expression.Parameter(x, $"variable{i}")).ToArray();
        ParameterExpression[] parameters = [.. stackValueParams, isSplitParam, .. variableParams];

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
                    Expression.NewArrayInit(
                        typeof(object),
                        variableParams.Select(
                            x => x.Type.IsValueType ? Expression.Convert(x, objectType) : (Expression)x
                        )
                    )
                )
            ).Compile();
    }

    private static IEnumerator DoStartNewGame(UI_NewGame uiNewGame)
    {

        AdaptableLog.Info("before DoStartNewGame");

        // DoStartNewGameOrigin(uiNewGame);

        AdaptableLog.Info("after DoStartNewGame");

        yield return new WaitForSeconds(3);

        AdaptableLog.Info("after WaitForSeconds");
    }

    private static Func<object[]>? BeforeRoll;
}