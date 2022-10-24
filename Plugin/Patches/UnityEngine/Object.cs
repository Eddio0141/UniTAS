using System;
using System.Reflection;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch(typeof(Object), nameof(Object.DontDestroyOnLoad))]
internal class DontDestroyOnLoad
{
    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static void Prefix(Object target)
    {
        GameTracker.DontDestroyOnLoadCall(target);
    }
}

[HarmonyPatch(typeof(Object), nameof(Object.Destroy), typeof(Object), typeof(float))]
internal class Destroy__Object__float
{
    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static void Prefix(Object obj)
    {
        GameTracker.DestroyObject(obj);
    }
}

[HarmonyPatch(typeof(Object), nameof(Object.DestroyImmediate), typeof(Object), typeof(bool))]
internal class DestroyImmediate__Object__bool
{
    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static void Prefix(Object obj)
    {
        GameTracker.DestroyObject(obj);
    }
}