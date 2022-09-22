using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine.__SceneManagment;

static class Helper
{
    const string NAMESPACE = "UnityEngine.SceneManagement";

    public static Type GetSceneManager()
    {
        return AccessTools.TypeByName($"{NAMESPACE}.SceneManager");
    }
}

[HarmonyPatch]
class UnloadSceneNameIndexInternal
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(Helper.GetSceneManager(), "UnloadSceneNameIndexInternal");
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(ref bool immediately, ref bool __state)
    {
        //__state = immediately;
        //immediately = true;
    }

    static void Postfix(ref AsyncOperation __result, ref bool __state)
    {
        /*
        if (!__state)
            GameTracker.AsyncSceneUnload(__result);
        Plugin.Log.LogDebug($"unload scene async post result {__result.progress}, {__result.isDone}, {__state}, {FakeGameState.GameTime.FrameCount}");
        */
    }
}

[HarmonyPatch]
class LoadSceneAsyncNameIndexInternal
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(Helper.GetSceneManager(), "LoadSceneAsyncNameIndexInternal");
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(ref bool mustCompleteNextFrame, ref bool __state, string sceneName, int sceneBuildIndex)
    {
        //Plugin.Log.LogDebug($"load scene async \"{sceneName}\", {sceneBuildIndex}, {mustCompleteNextFrame}");
        //__state = mustCompleteNextFrame;
        //mustCompleteNextFrame = true;
    }

    static void Postfix(ref AsyncOperation __result, ref bool __state)
    {
        Plugin.Log.LogDebug("load scene async");
        // no action if mustCompleteNextFrame is true
        /*
        if (__state)
            return;

        Plugin.Log.LogDebug($"tracking async load {Traverse.Create(__result).Field("m_Ptr").GetValue<IntPtr>()}");
        GameTracker.AsyncSceneLoad(__result);
        Plugin.Log.LogDebug($"load scene async post result, isDone: {__result.isDone}, priority: {__result.priority}, progress: {__result.progress}, {FakeGameState.GameTime.FrameCount}");
        */
    }
}