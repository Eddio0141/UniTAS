using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

// TODO different unity version investigation
[HarmonyPatch(typeof(AsyncOperation), "allowSceneActivation", MethodType.Setter)]
class setAllowSceneActivation
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_NeedsToBePatched(original, ex);
    }

    static void Prefix(bool value, AsyncOperation __instance)
    {
        Plugin.Log.LogDebug("allowSceneActivation set");
        /*
        if (value)
        {
            GameTracker.AllowSceneActivationSetTrue();
            Plugin.Log.LogDebug($"allowSceneActivation set true, calling for instance {Traverse.Create(__instance).Field("m_Ptr").GetValue<System.IntPtr>()}");
            GameTracker.AsyncOperationDestroy(__instance);
            // TODO make sure this doesn't get called more than once
            //Traverse.Create(__instance).Method("InvokeCompletionEvent").GetValue(new object[] { __instance });
            return;
        }
        else
        {
            GameTracker.AllowSceneActivationSetFalse(__instance);
            Plugin.Log.LogDebug("allowSceneActivation false");
        }
        */
    }
}

[HarmonyPatch(typeof(AsyncOperation), nameof(AsyncOperation.isDone), MethodType.Getter)]
class isDone
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_NeedsToBePatched(original, ex);
    }

    static void Prefix(ref bool __result, AsyncOperation __instance)
    {
        Plugin.Log.LogDebug("isDone get");
        /*
        Plugin.Log.LogDebug($"isDone getter for {Traverse.Create(__instance).Field("m_Ptr").GetValue<System.IntPtr>()}");
        // TODO unity version investigation
        var m_Ptr = Traverse.Create(__instance).Field("m_Ptr").GetValue<System.IntPtr>();
        if (GameTracker.AsyncSceneLoadsNoActivation.Contains(m_Ptr) || GameTracker.AsyncSceneUnloadsNoActivation.Contains(m_Ptr))
        {
            __result = false;
            Plugin.Log.LogDebug("isDone override to false");
            return false;
        }

        __result = true;
        return false;
        */
    }
}

[HarmonyPatch(typeof(AsyncOperation), nameof(AsyncOperation.progress), MethodType.Getter)]
class progress
{
    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_NeedsToBePatched(original, ex);
    }

    static void Prefix(ref float __result, AsyncOperation __instance)
    {
        Plugin.Log.LogDebug("progress getter");
        /*
        Plugin.Log.LogDebug($"progress getter for {Traverse.Create(__instance).Field("m_Ptr").GetValue<System.IntPtr>()}");
        // TODO unity version investigation
        var m_Ptr = Traverse.Create(__instance).Field("m_Ptr").GetValue<System.IntPtr>();
        if (GameTracker.AsyncSceneLoadsNoActivation.Contains(m_Ptr))
        {
            __result = 0.9f;
            Plugin.Log.LogDebug($"progress override to {__result}");
            return false;
        }

        __result = 1f;
        return false;
        */
    }
}

[HarmonyPatch]
class Finalize
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(AsyncOperation), "Finalize");
    }

    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_NeedsToBePatched(original, ex);
    }

    static void Prefix(AsyncOperation __instance)
    {
        Plugin.Log.LogDebug("deconstructor");
        //Plugin.Log.LogDebug($"destroying instance {Traverse.Create(__instance).Field("m_Ptr").GetValue<System.IntPtr>()}");
        //GameTracker.AsyncOperationDestroy(__instance);
    }
}

[HarmonyPatch]
class InvokeCompletionEvent
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(typeof(AsyncOperation), "InvokeCompletionEvent");
    }

    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_NeedsToBePatched(original, ex);
    }

    static void Prefix(AsyncOperation __instance)
    {
        Plugin.Log.LogDebug("InvokeCompletionEvent()");
        // TODO unity version
        /*
        var m_Ptr = Traverse.Create(__instance).Field("m_Ptr").GetValue<System.IntPtr>();
        var m_completeCallback = Traverse.Create(__instance).Field("m_completeCallback").GetValue<System.Action<AsyncOperation>>();
        if (m_completeCallback != null)
        {
            var delegateList = m_completeCallback.GetInvocationList();
            foreach (var del in delegateList)
            {
                Plugin.Log.LogDebug($"delegate {del.Method.Name}");
            }
        }*/
        /*
        Plugin.Log.LogDebug($"InvokeCompletionEvent for {m_Ptr}");
        if (GameTracker.AsyncSceneLoadsNoActivation.Contains(m_Ptr) || GameTracker.AsyncSceneUnloadsNoActivation.Contains(m_Ptr))
        {
            Plugin.Log.LogDebug($"skipping completion event call for instance {m_Ptr}");
            return false;
        }

        return true;
        */
    }
}

[HarmonyPatch]
class completed
{
    static MethodBase TargetMethod()
    {
        return typeof(AsyncOperation).GetEvent("completed", AccessTools.all).GetAddMethod();
    }

    static System.Exception Cleanup(MethodBase original, System.Exception ex)
    {
        return AuxilaryHelper.Cleanup_NeedsToBePatched(original, ex);
    }

    static void Prefix(System.Action<AsyncOperation> value, AsyncOperation __instance)
    {
        Plugin.Log.LogDebug("completed event add");
        //Plugin.Log.LogDebug($"completed: adding method {value.Method.Name} to {Traverse.Create(__instance).Field("m_Ptr").GetValue<System.IntPtr>()}");
        //Plugin.Log.LogDebug($"isDone is {__instance.isDone}, if true then that means value is being called");
    }
}