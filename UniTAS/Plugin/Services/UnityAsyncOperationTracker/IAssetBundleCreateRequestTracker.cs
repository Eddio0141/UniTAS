using UnityEngine;

namespace UniTAS.Plugin.Services.UnityAsyncOperationTracker;

public interface IAssetBundleCreateRequestTracker
{
    void NewAssetBundleCreateRequest(object asyncOperation, object assetBundle);
    AssetBundle GetAssetBundleCreateRequest(object asyncOperation);
}