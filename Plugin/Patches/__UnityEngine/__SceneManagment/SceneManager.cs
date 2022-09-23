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
        var asyncOrNot = immediately ? "immediately" : "async";
        Plugin.Log.LogDebug($"unload scene {asyncOrNot} at {FakeGameState.GameTime.FrameCount}");
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

    static bool Prefix(bool mustCompleteNextFrame, ref bool __state, string sceneName, int sceneBuildIndex, object parameters, ref AsyncOperation __result)
    {
        var loadSceneMode = Traverse.Create(parameters).Property("loadSceneMode").GetValue();
        var asyncOrNot = mustCompleteNextFrame ? "insta" : "async";
        Plugin.Log.LogDebug($"load scene {asyncOrNot}, mode: {loadSceneMode} at {FakeGameState.GameTime.FrameCount}");
        if (!mustCompleteNextFrame)
        {
            __result = new AsyncOperation();
            GameTracker.AsyncSceneLoad(sceneName, sceneBuildIndex, parameters);
            return false;
        }
        return true;
    }
}