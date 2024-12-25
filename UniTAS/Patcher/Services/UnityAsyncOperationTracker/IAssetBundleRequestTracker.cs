using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface IAssetBundleRequestTracker
{
    void NewAssetBundleRequest(AsyncOperation op, Object obj);
    void NewAssetBundleRequestMultiple(AsyncOperation op, AssetBundle bundle, string name, Type type);

    bool GetAssetBundleRequest(AsyncOperation asyncOperation, out Object obj);
    bool GetAssetBundleRequestMultiple(AsyncOperation asyncOperation, out Object[] objects);
}