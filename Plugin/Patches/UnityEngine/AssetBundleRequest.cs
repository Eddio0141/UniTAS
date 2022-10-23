using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch(typeof(AssetBundleRequest), nameof(AssetBundleRequest.asset), MethodType.Getter)]
class get_asset
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(AssetBundleRequest __instance, ref global::UnityEngine.Object __result)
    {
        var wrap = new AsyncOperationWrap(__instance);
        var runOriginal = !AssetBundleRequestWrap.InstanceTracker.TryGetValue(wrap.UID, out var foundResult);
        if (!runOriginal)
            __result = foundResult.Key;
        return runOriginal;
    }
}

[HarmonyPatch(typeof(AssetBundleRequest), "allAssets", MethodType.Getter)]
class get_allAssets
{
    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(AssetBundleRequest __instance, ref global::UnityEngine.Object[] __result)
    {
        var wrap = new AsyncOperationWrap(__instance);
        var runOriginal = !AssetBundleRequestWrap.InstanceTracker.TryGetValue(wrap.UID, out var foundResult);
        if (!runOriginal)
            __result = foundResult.Value;
        return runOriginal;
    }
}