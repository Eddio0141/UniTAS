using System;
using UnityEngine;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface IAssetBundleRequestTracker
{
    void NewAssetBundleRequest(AsyncOperation op, AssetBundle bundle, string name, Type type);
    void NewAssetBundleRequestMultiple(AsyncOperation op, AssetBundle bundle, string name, Type type);

    object GetAssetBundleRequest(AsyncOperation asyncOperation);
    object GetAssetBundleRequestMultiple(AsyncOperation asyncOperation);
}