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

    static void Prefix(ref bool immediately)
    {
        immediately = true;
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
            GameTracker.AsyncSceneLoad(sceneName, sceneBuildIndex, parameters, __result);
            return false;
        }
        return true;
    }
}