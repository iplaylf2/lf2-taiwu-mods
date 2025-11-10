using Cysharp.Threading.Tasks;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using HarmonyLib;
using LF2.Cecil.Helper;
using LF2.Frontend.Helper;
using LF2.Game.Helper;
using LF2.Kit.Service;
using MonoMod.Cil;
using RollProtagonist.Common;
using RollProtagonist.Frontend.NewGamePlus.Core;
using System.Reflection;

namespace RollProtagonist.Frontend.NewGamePlus.Patches;

[HarmonyPatch(typeof(UI_NewGame))]
internal static class UI_NewGamePatch
{
    [HarmonyPrepare]
    [HarmonyPatch(nameof(UI_NewGame.DoStartNewGame))]
    private static void DoStartNewGameTap(MethodBase originMethod)
    {
        StructuredLogger.Info("DoStartNewGameTap started");

        var CharacterDisplay = ModResourceFactory.CreateModCopy
        (
            () => new()
            {
                _path = UIElement.MouseTipCharacterComplete._path
            }
        );

        static IEnumerable<Type> BeforeRollSplitPoint(ILCursor ilCursor)
        {
            var createProtagonist = CharacterDomainMethod.Call.CreateProtagonist;

            _ = ilCursor
            .GotoNext(x => x.MatchCallOrCallvirt(createProtagonist.GetMethodInfo()))
            .Remove();

            return
            [
                typeof(int),
                typeof(ProtagonistCreationInfo)
            ];
        }

        static void AfterRollContinuationPoint(ILCursor ilCursor)
        {
            var createProtagonist = CharacterDomainMethod.Call.CreateProtagonist;

            _ = ilCursor.GotoNext(x => x.MatchCallOrCallvirt(createProtagonist.GetMethodInfo()));
            ilCursor.Index++;
        }

        var beforeRoll = MethodSegmenter.CreateLeftSegment<Func<UI_NewGame, Tuple<object[], bool, object[]>>>
        (
            (MethodInfo)originMethod,
            BeforeRollSplitPoint
        );

        StructuredLogger.Info("method generated", new { method = nameof(beforeRoll) });

        var afterRoll = MethodSegmenter.CreateRightSegment<Action<UI_NewGame, object[]>>
        (
            (MethodInfo)originMethod,
            AfterRollContinuationPoint
        );

        StructuredLogger.Info("method generated", new { method = nameof(afterRoll) });

        _ = ModServiceRegistry.TryGet(out ModConfig? config);

        _ = ModServiceRegistry.Add
        (
            () => new NewGameRollCoordinator(config!.ModId, beforeRoll, afterRoll, CharacterDisplay!)
        );
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(UI_NewGame.DoStartNewGame))]
    private static bool DoStartNewGamePrefix(UI_NewGame __instance)
    {
        _ = ModServiceRegistry.TryGet(out NewGameRollCoordinator? service);

        service!.Execute(__instance).Forget();

        return false;
    }
}
