using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.LegacySafeWrappers;
using AssetBundleOrig = UnityEngine.AssetBundle;
using AssetBundleCreateRequestOrig = UnityEngine.AssetBundleCreateRequest;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable SuggestBaseTypeForParameter

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch]
internal static class AssetBundleCreateRequestPatch
{
    [HarmonyPatch(typeof(AssetBundleCreateRequestOrig), nameof(AssetBundleCreateRequestOrig.assetBundle),
        MethodType.Getter)]
    internal class get_assetBundle
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(AssetBundleCreateRequestOrig __instance, ref AssetBundleOrig __result)
        {
            var wrap = new AsyncOperationWrap(__instance);
            return !AssetBundleCreateRequestWrap.InstanceTracker.TryGetValue(wrap.UID, out __result);
        }
    }
}