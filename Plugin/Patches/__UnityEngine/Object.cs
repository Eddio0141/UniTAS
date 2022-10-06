using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

[HarmonyPatch(typeof(Object), nameof(Object.DontDestroyOnLoad))]
class DontDestroyOnLoad
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(Object target)
    {
        GameTracker.DontDestroyOnLoadCall(target);
    }
}

[HarmonyPatch(typeof(Object), nameof(Object.Destroy), new System.Type[] { typeof(Object), typeof(float) })]
class Destroy__Object__float
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(Object obj)
    {
        GameTracker.DestroyObject(obj);
    }
}

[HarmonyPatch(typeof(Object), nameof(Object.DestroyImmediate), new System.Type[] { typeof(Object), typeof(bool) })]
class DestroyImmediate__Object__bool
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return Patches.PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(Object obj)
    {
        GameTracker.DestroyObject(obj);
    }
}