using UnityEngine;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface IAssetBundleCreateRequestTracker
{
    void NewAssetBundleCreateRequest(AsyncOperation asyncOperation, AssetBundle assetBundle);
    AssetBundle GetAssetBundleCreateRequest(AsyncOperation asyncOperation);
}