using HarmonyLib;
using FrameWork.AssetBundlePackage;

namespace RollProtagonist.Frontend
{
    [HarmonyPatch(typeof(ResourcePackage), nameof(ResourcePackage.TryGetAssetBundleLoadData))]
    internal static class AssetBundlePatcher
    {
        public delegate void AfterLoadAssetBundleHandler(Type baseType, string assetName, UnityEngine.Object asset);

        public static event AfterLoadAssetBundleHandler AfterLoadAssetBundle = delegate { };

        private static void Postfix(
            Type type,
            string assetName,
            ref (ResourcePackage package, string bundleName, UnityEngine.Object asset) __result)
        {
            AfterLoadAssetBundle(type, assetName, __result.asset);
        }
    }
}
