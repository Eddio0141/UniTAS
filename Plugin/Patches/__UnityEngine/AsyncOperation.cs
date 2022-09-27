using HarmonyLib;
using System.Reflection;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

// TODO different unity version investigation
[HarmonyPatch(typeof(AsyncOperation), "allowSceneActivation", MethodType.Setter)]
class setAllowSceneActivation
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(bool value, AsyncOperation __instance)
    {
        GameTracker.AllowSceneActivation(value, __instance);
        return false;
    }
}

[HarmonyPatch(typeof(AsyncOperation), "allowSceneActivation", MethodType.Getter)]
class getAllowSceneActivation
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref bool __result, AsyncOperation __instance)
    {
        __result = GameTracker.GetSceneActivation(__instance);
        return false;
    }
}

// TODO probably good idea to override priority

[HarmonyPatch(typeof(AsyncOperation), nameof(AsyncOperation.progress), MethodType.Getter)]
class progress
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref float __result, AsyncOperation __instance)
    {
        __result = GameTracker.IsStallingInstance(__instance) ? 0.9f : 1f;
        return false;
    }
}

[HarmonyPatch(typeof(AsyncOperation), nameof(AsyncOperation.isDone), MethodType.Getter)]
class isDone
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(ref bool __result, AsyncOperation __instance)
    {
        __result = !GameTracker.IsStallingInstance(__instance);
        return false;
    }
}

[HarmonyPatch(typeof(AsyncOperation), "InternalDestroy")]
class InternalDestroy
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(AsyncOperation __instance)
    {
        // unless UID is 0, we shouldn't let it proceed
        var wrap = new AsyncOperationWrap(__instance);
        wrap.FinalizeCall();
    }
}