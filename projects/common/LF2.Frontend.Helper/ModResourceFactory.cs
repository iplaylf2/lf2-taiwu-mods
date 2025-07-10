using HarmonyLib;
using UnityEngine;

namespace LF2.Frontend.Helper;

public static class ModResourceFactory
{
    public class ModdedUIBehavior : MonoBehaviour { }

    /// <summary>
    /// Creates a mod-specific copy of a UI element by loading its original prefab.
    /// </summary>
    /// <param name="originalElement">The live UI element instance to use as a reference.</param>
    /// <param name="onInstantiated">Callback with the new instance, or null on failure.</param>
    public static void CreateModCopy(UIElement originalElement, Action<GameObject?> onInstantiated)
    {
        string uiPath = GetPathFromOriginal(originalElement);

        if (string.IsNullOrEmpty(uiPath))
        {
            Debug.LogError(
                $"[ModResourceFactory] Error: Could not retrieve uiPath from originalElement: {originalElement.Name}"
            );

            onInstantiated(null);

            return;
        }

        string fullPath = Path.Combine(rootPrefabPath, uiPath);

        ResLoader.Load<GameObject>(fullPath, (prefab) =>
        {
            if (prefab == null)
            {
                Debug.LogError($"[ModResourceFactory] Failed to load game UI prefab at path: {fullPath}");

                onInstantiated(null);

                return;
            }

            GameObject modInstance = UnityEngine.Object.Instantiate(prefab);

            modInstance.name = $"{prefab.name}_ModCopy";

            if (UIManager.Instance?.transform != null)
            {
                modInstance.transform.SetParent(UIManager.Instance.transform, false);
            }
            else
            {
                Debug.LogWarning(
                    "[ModResourceFactory] UIManager.Instance is not available. UI may not be parented correctly."
                );
            }

            modInstance.AddComponent<ModdedUIBehavior>();

            onInstantiated(modInstance);
        });
    }

    private static string GetPathFromOriginal(UIElement originalElement)
    {
        return Traverse.Create(originalElement).Field("_path").GetValue<string>();
    }

    private static readonly string rootPrefabPath =
        Traverse.Create(typeof(UIElement)).Field("rootPrefabPath").GetValue<string>();
}
