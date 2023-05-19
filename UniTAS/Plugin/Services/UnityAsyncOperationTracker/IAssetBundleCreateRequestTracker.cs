using UnityEngine;

namespace UniTAS.Plugin.Services.UnityAsyncOperationTracker;

public interface IAssetBundleCreateRequestTracker
{
    void NewAssetBundleCreateRequest(AsyncOperation asyncOperation, AssetBundle assetBundle);
    AssetBundle GetAssetBundleCreateRequest(AsyncOperation asyncOperation);
}