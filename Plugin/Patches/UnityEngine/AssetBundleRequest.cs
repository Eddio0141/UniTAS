using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch(typeof(AssetBundleRequest), nameof(AssetBundleRequest.asset), MethodType.Getter)]
internal class get_asset
{
    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static bool Prefix(AssetBundleRequest __instance, ref Object __result)
    {
        var wrap = new AsyncOperationWrap(__instance);
        var runOriginal = !AssetBundleRequestWrap.InstanceTracker.TryGetValue(wrap.UID, out var foundResult);
        if (!runOriginal)
            __result = foundResult.Key;
        return runOriginal;
    }
}

[HarmonyPatch(typeof(AssetBundleRequest), "allAssets", MethodType.Getter)]
internal class get_allAssets
{
    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static bool Prefix(AssetBundleRequest __instance, ref Object[] __result)
    {
        var wrap = new AsyncOperationWrap(__instance);
        var runOriginal = !AssetBundleRequestWrap.InstanceTracker.TryGetValue(wrap.UID, out var foundResult);
        if (!runOriginal)
            __result = foundResult.Value;
        return runOriginal;
    }
}