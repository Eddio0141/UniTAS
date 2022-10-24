using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch(typeof(AssetBundleCreateRequest), nameof(AssetBundleCreateRequest.assetBundle), MethodType.Getter)]
internal class get_assetBundle
{
    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static bool Prefix(AssetBundleCreateRequest __instance, ref AssetBundle __result)
    {
        var wrap = new AsyncOperationWrap(__instance);
        return !AssetBundleCreateRequestWrap.InstanceTracker.TryGetValue(wrap.UID, out __result);
    }
}