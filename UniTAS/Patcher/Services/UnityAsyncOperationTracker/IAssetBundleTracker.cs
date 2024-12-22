using UnityEngine;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface IAssetBundleTracker
{
    void Unload(AssetBundle assetBundle);
    void UnloadBundleAsync(AsyncOperation op);
}