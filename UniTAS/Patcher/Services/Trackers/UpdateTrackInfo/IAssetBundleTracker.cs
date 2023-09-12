using UnityEngine;

namespace UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;

public interface IAssetBundleTracker
{
    void NewInstance(AssetBundle assetBundle);
    void NewInstance(AssetBundleCreateRequest assetBundleCreateRequest);
}