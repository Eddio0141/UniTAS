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

    static void Prefix(bool value)
    {
        Plugin.Log.LogDebug($"allowSceneActivation set to {value} at {FakeGameState.GameTime.FrameCount}");
        GameTracker.AllowSceneActivation(value);
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
        // isDone getters are used but all returns false as far as i see (unknown if its the game or unity calling)
        // doesn't matter what gets returned unless game wants to use it
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
        // progress getter are never used in unity
        // doesn't matter what gets returned unless game wants to use it
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
        Plugin.Log.LogDebug($"InvokeCompletionEvent at {FakeGameState.GameTime.FrameCount}");
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
        Plugin.Log.LogDebug($"completed event add at {FakeGameState.GameTime.FrameCount}");
    }
}