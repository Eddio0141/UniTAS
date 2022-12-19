using System.Collections.Generic;
using UnityEngine;

namespace UniTASPlugin.SafeWrappers;

internal static class AssetBundleRequestWrap
{
    public static Dictionary<ulong, KeyValuePair<Object, Object[]>> InstanceTracker { get; private set; } = new();

    public static void NewFakeInstance(AsyncOperationWrap wrap, Object obj)
    {
        InstanceTracker.Add(wrap.UID, new(obj, null));
    }

    public static void NewFakeInstance(AsyncOperationWrap wrap, Object[] objs)
    {
        InstanceTracker.Add(wrap.UID, new(null, objs));
    }

    public static void FinalizeCall(ulong uid)
    {
        _ = InstanceTracker.Remove(uid);
    }
}