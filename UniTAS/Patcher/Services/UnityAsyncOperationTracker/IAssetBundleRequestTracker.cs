using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface IAssetBundleRequestTracker
{
    void NewAssetBundleRequest(AsyncOperation op, AssetBundle assetBundle, string name, Type type);
    void NewAssetBundleRequestMultiple(AsyncOperation op, Object[] objs);

    bool GetAssetBundleRequest(AsyncOperation asyncOperation, out Object obj);
    bool GetAssetBundleRequestMultiple(AsyncOperation asyncOperation, out Object[] objects);
}