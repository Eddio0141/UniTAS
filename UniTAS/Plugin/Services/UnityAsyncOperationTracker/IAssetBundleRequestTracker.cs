namespace UniTAS.Plugin.Services.UnityAsyncOperationTracker;

public interface IAssetBundleRequestTracker
{
    void NewAssetBundleRequest(object asyncOperation, object assetBundleRequest);
    void NewAssetBundleRequestMultiple(object asyncOperation, object assetBundleRequestArray);

    object GetAssetBundleRequest(object asyncOperation);
    object GetAssetBundleRequestMultiple(object asyncOperation);
}