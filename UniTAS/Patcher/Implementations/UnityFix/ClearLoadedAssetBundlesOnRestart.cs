using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnityFix;

[Singleton]
public class ClearLoadedAssetBundlesOnRestart : IAssetBundleTracker, IOnPreGameRestart
{
    private readonly List<AssetBundle> _trackedAssetBundles = new();
    private readonly List<AssetBundleCreateRequest> _trackedAssetBundleCreateRequests = new();

    public void NewInstance(AssetBundle assetBundle)
    {
        _trackedAssetBundles.Add(assetBundle);
    }


    public void NewInstance(AssetBundleCreateRequest assetBundleCreateRequest)
    {
        _trackedAssetBundleCreateRequests.Add(assetBundleCreateRequest);
    }

    public void OnPreGameRestart()
    {
        foreach (var assetBundle in _trackedAssetBundles)
        {
            if (assetBundle == null) continue;
            assetBundle.Unload(true);
        }

        foreach (var assetBundleCreateRequest in _trackedAssetBundleCreateRequests)
        {
            if (assetBundleCreateRequest == null || assetBundleCreateRequest.assetBundle == null) continue;
            assetBundleCreateRequest.assetBundle.Unload(true);
        }

        _trackedAssetBundles.Clear();
        _trackedAssetBundleCreateRequests.Clear();
    }
}