using UnityEngine;

namespace LF2.Frontend.Helper;

public static class ModResourceFactory
{
    public class ModdedUIBehavior : MonoBehaviour;

    public static UIElement CreateModCopy(Func<UIElement> factory, bool autoShow = false)
    {
        var copy = factory();

        copy.PrepareRes(autoShow);
        copy.UiBase.gameObject.AddComponent<ModdedUIBehavior>();

        return copy;
    }
}
