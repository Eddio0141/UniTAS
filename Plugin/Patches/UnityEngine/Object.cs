using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch(typeof(Object), nameof(Object.DontDestroyOnLoad))]
class DontDestroyOnLoad
{
    static global::System.Exception Cleanup(MethodBase original, global::System.Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(Object target)
    {
        GameTracker.DontDestroyOnLoadCall(target);
    }
}

[HarmonyPatch(typeof(Object), nameof(Object.Destroy), new global::System.Type[] { typeof(Object), typeof(float) })]
class Destroy__Object__float
{
    static global::System.Exception Cleanup(MethodBase original, global::System.Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(Object obj)
    {
        GameTracker.DestroyObject(obj);
    }
}

[HarmonyPatch(typeof(Object), nameof(Object.DestroyImmediate), new global::System.Type[] { typeof(Object), typeof(bool) })]
class DestroyImmediate__Object__bool
{
    static global::System.Exception Cleanup(MethodBase original, global::System.Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(Object obj)
    {
        GameTracker.DestroyObject(obj);
    }
}