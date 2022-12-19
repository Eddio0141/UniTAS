using System.Collections.Generic;
using UnityEngine;

namespace UniTASPlugin.SafeWrappers;

internal static class AssetBundleCreateRequestWrap
{
    public static Dictionary<ulong, AssetBundle> InstanceTracker { get; private set; } = new();

    public static void NewFakeInstance(AsyncOperationWrap wrap, AssetBundle assetBundle)
    {
        InstanceTracker.Add(wrap.UID, assetBundle);
    }

    public static void FinalizeCall(ulong uid)
    {
        _ = InstanceTracker.Remove(uid);
    }
}