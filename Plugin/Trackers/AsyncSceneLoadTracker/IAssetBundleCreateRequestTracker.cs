namespace UniTASPlugin.Trackers.AsyncSceneLoadTracker;

public interface IAssetBundleCreateRequestTracker
{
    void NewAssetBundleCreateRequest(object asyncOperation, object assetBundle);
    object GetAssetBundleCreateRequest(object asyncOperation);
}