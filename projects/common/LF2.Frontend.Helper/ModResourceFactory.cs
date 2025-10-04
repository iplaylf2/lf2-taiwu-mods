using GameData.Utilities;
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
        _ = element.UiBase.gameObject.AddComponent<ModdedUIBehavior>();
    }

    private static void PrepareRes(UIElement instance, bool autoShow)
    {
        var _path = instance._path;
        var UiBase = instance.UiBase;
        var _stateMachine = instance._stateMachine;
        var rootPrefabPath = UIElement.rootPrefabPath;

        if (_path.IsNullOrEmpty())
        {
            GLog.TagError
            (
                "UIElement",
                "Fetal Error:UIElement type="
                + instance.GetType().FullName
                + " set empty prefab path!"
            );
        }
        else if (UiBase)
        {
            if (autoShow)
            {
                _stateMachine.TranslateState(EUiElementState.Reset);
            }
        }
        else
        {

            ResLoader.Load<GameObject>(Path.Combine(rootPrefabPath, _path), OnPrefabLoaded);
        }

        void OnPrefabLoaded(GameObject obj)
        {
            if (obj == null)
            {
                AdaptableLog.Warning
                (
                    "PrepareRes load "
                    + _path
                    + " failed because prefab is null!",
                    appendWarningMessage: true
                );
            }
            else
            {
                var component = obj.GetComponent<UIBase>();
                if (component == null)
                {
                    AdaptableLog.Warning
                    (
                        "PrepareRes load "
                        + _path
                        + " failed because prefab missing scripts!",
                        appendWarningMessage: true
                    );
                }
                else
                {
                    var Name = instance.Name;

                    bool activeSelf = obj.activeSelf;
                    obj.SetActive(value: false);
                    UiBase = UnityEngine.Object.Instantiate(component);
                    UiBase.gameObject.name = Name;
                    OnUIBaseLoaded(instance);
                    UIManager.Instance.PlaceUI(UiBase);
                    UiBase.Element = instance;
                    var component2 = UiBase.GetComponent<ConchShipGraphicRaycaster>();
                    if (component2)
                    {
                        component2.TargetCamera = UIManager.Instance.UiCamera;
                    }

                    UiBase.RegisterRelativeAtlases();
                    var componentsInChildren = UiBase.GetComponentsInChildren<CButton>(includeInactive: true);
                    componentsInChildren.ForEach
                    (
                        (_, btn) =>
                        {
                            if (btn.AutoListen)
                            {
                                btn.ClearAndAddListener
                                (
                                    delegate
                                    {
                                        UiBase.HandleClick(btn);
                                    }
                                );
                            }

                            return false;
                        }
                    );
                    if (autoShow)
                    {
                        _stateMachine.TranslateState(EUiElementState.Reset);
                    }

                    obj.SetActive(activeSelf);
                }
            }
        }
    }
}
