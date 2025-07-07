using UnityEngine;

namespace LF2.Frontend.Helper;

public static class GameResourceHelper
{
    private const string GameUiRootPath = "RemakeResources/Prefab/Views/";

    public static void LoadAndInstantiateGameUI(string uiPath, Action<GameObject?> onInstantiated)
    {
        if (string.IsNullOrEmpty(uiPath))
        {
            Debug.LogError("[GameResourceHelper] Error: uiPath cannot be null or empty.");

            onInstantiated?.Invoke(null);

            return;
        }

        string fullPath = Path.Combine(GameUiRootPath, uiPath);

        ResLoader.Load<GameObject>(fullPath, (prefab) =>
        {
            if (prefab == null)
            {
                Debug.LogError($"[GameResourceHelper] Failed to load game UI prefab at path: {fullPath}");

                onInstantiated?.Invoke(null);

                return;
            }

            GameObject uiInstance = UnityEngine.Object.Instantiate(prefab);

            uiInstance.name = prefab.name + "_ModInstance";

            if (UIManager.Instance != null && UIManager.Instance.transform != null)
            {
                uiInstance.transform.SetParent(UIManager.Instance.transform, false);
            }
            else
            {
                Debug.LogError("[GameResourceHelper] UIManager.Instance is not available. Cannot place the UI correctly.");
            }

            onInstantiated?.Invoke(uiInstance);
        });
    }
}