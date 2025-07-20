using GameData.Utilities;
using HarmonyLib;
using UnityEngine;

namespace LF2.Frontend.Helper;

public static class ModResourceFactory
{
    public class ModdedUIBehavior : MonoBehaviour;

    public static UIElement CreateModCopy(Func<UIElement> factory, bool autoShow = false)
    {
        var copy = factory();

        PrepareRes(copy, autoShow);

        return copy;
    }

    private static void OnUIBaseLoaded(UIElement element)
    {
        element.UiBase.name += "_modded_ui";
        element.UiBase.gameObject.AddComponent<ModdedUIBehavior>();
    }

    private static void PrepareRes(UIElement instance, bool autoShow)
    {
        var element = Traverse.Create(instance);
        var path = element.Field("_path").GetValue<string>();
        var stateMachine = element.Field("_stateMachine").GetValue<StateMachine>();
        var UIElementType = Traverse.Create<UIElement>();

        if (path.IsNullOrEmpty())
        {
            GLog.TagError(nameof(UIElement), $"Fetal Error:UIElement type={instance.GetType().FullName} set empty prefab path!");
        }
        else if (instance.UiBase != null)
        {
            if (!autoShow)
            {
                return;
            }

            stateMachine.TranslateState(EUiElementState.Reset);
        }
        else
        {
            ResLoader.Load(
                Path.Combine(UIElementType.Field("rootPrefabPath").GetValue<string>(), path),
                new Action<GameObject>(OnPrefabLoaded)
            );
        }

        void OnPrefabLoaded(GameObject obj)
        {
            if (obj == null)
            {
                AdaptableLog.Warning($"PrepareRes load {path} failed because prefab is null!", true);
            }
            else
            {
                UIBase component1 = obj.GetComponent<UIBase>();
                if (component1 == null)
                {
                    AdaptableLog.Warning($"PrepareRes load {path} failed because prefab missing scripts!", true);
                }
                else
                {
                    bool activeSelf = obj.activeSelf;
                    obj.SetActive(false);
                    instance.UiBase = UnityEngine.Object.Instantiate(component1);
                    instance.UiBase.gameObject.name = instance.Name;

                    OnUIBaseLoaded(instance);

                    UIManager.Instance.PlaceUI(instance.UiBase);
                    instance.UiBase.Element = instance;
                    ConchShipGraphicRaycaster component2 = instance.UiBase.GetComponent<ConchShipGraphicRaycaster>();
                    if (component2 != null)
                    {
                        component2.TargetCamera = UIManager.Instance.UiCamera;
                    }

                    instance.UiBase.RegisterRelativeAtlases();
                    instance.UiBase.GetComponentsInChildren<CButton>(true).ForEach((_, btn) =>
                    {
                        if (btn.AutoListen)
                        {
                            btn.ClearAndAddListener(() => instance.UiBase.HandleClick(btn));
                        }

                        return false;
                    });
                    if (autoShow)
                    {
                        stateMachine.TranslateState(EUiElementState.Reset);
                    }

                    obj.SetActive(activeSelf);
                }
            }
        }
    }
}
