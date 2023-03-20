namespace UniTAS.Plugin.Services.UnityAsyncOperationTracker;

public interface IAssetBundleCreateRequestTracker
{
    void NewAssetBundleCreateRequest(object asyncOperation, object assetBundle);
    object GetAssetBundleCreateRequest(object asyncOperation);
}