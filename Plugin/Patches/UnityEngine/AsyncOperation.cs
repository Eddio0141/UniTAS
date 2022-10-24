using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin.Patches.UnityEngine;

// TODO different unity version investigation
[HarmonyPatch(typeof(AsyncOperation), "allowSceneActivation", MethodType.Setter)]
internal class setAllowSceneActivation
{
    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static bool Prefix(bool value, AsyncOperation __instance)
    {
        GameTracker.AllowSceneActivation(value, __instance);
        return false;
    }
}

[HarmonyPatch(typeof(AsyncOperation), "allowSceneActivation", MethodType.Getter)]
internal class getAllowSceneActivation
{
    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static bool Prefix(ref bool __result, AsyncOperation __instance)
    {
        __result = GameTracker.GetSceneActivation(__instance);
        return false;
    }
}

// TODO probably good idea to override priority

[HarmonyPatch(typeof(AsyncOperation), nameof(AsyncOperation.progress), MethodType.Getter)]
internal class progress
{
    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static bool Prefix(ref float __result, AsyncOperation __instance)
    {
        __result = GameTracker.IsStallingInstance(__instance) ? 0.9f : 1f;
        return false;
    }
}

[HarmonyPatch(typeof(AsyncOperation), nameof(AsyncOperation.isDone), MethodType.Getter)]
internal class isDone
{
    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static bool Prefix(ref bool __result, AsyncOperation __instance)
    {
        __result = !GameTracker.IsStallingInstance(__instance);
        return false;
    }
}

[HarmonyPatch(typeof(AsyncOperation), "InternalDestroy")]
internal class InternalDestroy
{
    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static void Prefix(AsyncOperation __instance)
    {
        // unless UID is 0, we shouldn't let it proceed
        var wrap = new AsyncOperationWrap(__instance);
        wrap.FinalizeCall();
    }
}