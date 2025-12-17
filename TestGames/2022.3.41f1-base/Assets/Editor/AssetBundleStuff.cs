using System.IO;
using UnityEditor;

namespace Editor
{
    public static class AssetBundleStuff
    {
        [MenuItem("Assets/Build AssetBundles")]
        private static void Build()
        {
            const string assetBundleDirectory = "Assets/StreamingAssets";
            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }

            BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None,
                BuildTarget.StandaloneLinux64);
        }
    }
}