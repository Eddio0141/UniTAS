using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.SafeWrappers;
using AsyncOpOrig = UnityEngine.AsyncOperation;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantAssignment

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch]
internal static class AsyncOperation
{
    // TODO different unity version investigation
    [HarmonyPatch(typeof(AsyncOpOrig), "allowSceneActivation", MethodType.Setter)]
    private class setAllowSceneActivation
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(bool value, AsyncOpOrig __instance)
        {
            GameTracker.AllowSceneActivation(value, __instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(AsyncOpOrig), "allowSceneActivation", MethodType.Getter)]
    private class getAllowSceneActivation
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result, AsyncOpOrig __instance)
        {
            __result = GameTracker.GetSceneActivation(__instance);
            return false;
        }
    }

    // TODO probably good idea to override priority

    [HarmonyPatch(typeof(AsyncOpOrig), nameof(AsyncOpOrig.progress), MethodType.Getter)]
    private class progress
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref float __result, AsyncOpOrig __instance)
        {
            __result = GameTracker.IsStallingInstance(__instance) ? 0.9f : 1f;
            return false;
        }
    }

    [HarmonyPatch(typeof(AsyncOpOrig), nameof(AsyncOpOrig.isDone), MethodType.Getter)]
    private class isDone
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static bool Prefix(ref bool __result, AsyncOpOrig __instance)
        {
            __result = !GameTracker.IsStallingInstance(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(AsyncOpOrig), "privateDestroy")]
    private class privateDestroy
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.Cleanup_IgnoreException(original, ex);
        }

        private static void Prefix(AsyncOpOrig __instance)
        {
            // unless UID is 0, we shouldn't let it proceed
            var wrap = new AsyncOperationWrap(__instance);
            wrap.FinalizeCall();
        }
    }
}