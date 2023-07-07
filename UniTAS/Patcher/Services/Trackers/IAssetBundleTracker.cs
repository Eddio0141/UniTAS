using UnityEngine;

namespace UniTAS.Patcher.Services.Trackers;

public interface IAssetBundleTracker
{
    void NewInstance(AssetBundle assetBundle);
    void NewInstance(AssetBundleCreateRequest assetBundleCreateRequest);
}