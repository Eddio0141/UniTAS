using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.LegacySafeWrappers;
using ObjectOrig = UnityEngine.Object;
using AssetBundleRequestOrig = UnityEngine.AssetBundleRequest;

// ReSharper disable SuggestBaseTypeForParameter
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch]
internal static class AssetBundleRequestPatch
{
    [HarmonyPatch(typeof(AssetBundleRequestOrig), nameof(AssetBundleRequestOrig.asset), MethodType.Getter)]
    private class get_asset
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(AssetBundleRequestOrig __instance, ref ObjectOrig __result)
        {
            var wrap = new AsyncOperationWrap(__instance);
            var runOriginal = !AssetBundleRequestWrap.InstanceTracker.TryGetValue(wrap.UID, out var foundResult);
            if (!runOriginal)
                __result = foundResult.Key;
            return runOriginal;
        }
    }

    [HarmonyPatch(typeof(AssetBundleRequestOrig), "allAssets", MethodType.Getter)]
    private class get_allAssets
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(AssetBundleRequestOrig __instance, ref ObjectOrig[] __result)
        {
            var wrap = new AsyncOperationWrap(__instance);
            var runOriginal = !AssetBundleRequestWrap.InstanceTracker.TryGetValue(wrap.UID, out var foundResult);
            if (!runOriginal)
                __result = foundResult.Value;
            return runOriginal;
        }
    }
}