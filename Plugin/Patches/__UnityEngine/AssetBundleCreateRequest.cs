using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

[HarmonyPatch(typeof(AssetBundleCreateRequest), nameof(AssetBundleCreateRequest.assetBundle), MethodType.Getter)]
class get_assetBundle
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(AssetBundleCreateRequest __instance, ref AssetBundle __result)
    {
        var wrap = new AsyncOperationWrap(__instance);
        return !AssetBundleCreateRequestWrap.InstanceTracker.TryGetValue(wrap.UID, out __result);
    }
}