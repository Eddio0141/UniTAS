﻿using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine.__SceneManagment;

static class Helper
{
    const string NAMESPACE = "UnityEngine.SceneManagement";

    public static Type GetSceneManager()
    {
        return AccessTools.TypeByName($"{NAMESPACE}.SceneManager");
    }

    public static Type GetUnloadSceneOptions()
    {
        return AccessTools.TypeByName($"{NAMESPACE}.UnloadSceneOptions");
    }

    public static MethodInfo GetUnloadSceneNameIndexInternal()
    {
        return AccessTools.Method(GetSceneManager(), "UnloadSceneNameIndexInternal", new Type[] { typeof(string), typeof(int), typeof(bool), GetUnloadSceneOptions(), typeof(bool) });
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
class UnloadSceneAsyncInternal_Injected
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(Helper.GetSceneManager(), "UnloadSceneAsyncInternal_Injected");
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(object scene, object options)
    {
        var sceneTraverse = Traverse.Create(scene);
        var sceneBuildIndex = sceneTraverse.Property("buildIndex").GetValue<int>();
        Helper.GetUnloadSceneNameIndexInternal().Invoke(null, new object[] { "", sceneBuildIndex, true, options, null });
        return false;
    }
}

[HarmonyPatch]
class LoadSceneAsyncNameIndexInternal__sceneName__sceneBuildIndex__isAdditive__mustCompleteNextFrame
{
    static MethodBase TargetMethod()
    {
        // string sceneName, int sceneBuildIndex, bool isAdditive, bool mustCompleteNextFrame
        return AccessTools.Method(Helper.GetSceneManager(), "LoadSceneAsyncNameIndexInternal", new Type[] { typeof(string), typeof(int), typeof(bool), typeof(bool) });
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(string sceneName, int sceneBuildIndex, bool isAdditive, bool mustCompleteNextFrame, ref AsyncOperation __result)
    {
        if (!mustCompleteNextFrame)
        {
            Plugin.Log.LogDebug($"async scene load");
            __result = new AsyncOperation();
            GameTracker.AsyncSceneLoad(sceneName, sceneBuildIndex, isAdditive, ref __result);
            Plugin.Log.LogDebug($"setting up async scene load, assigned UID {new VersionSafeWrapper.AsyncOperationWrap(__result).UID}");
            return false;
        }
        return true;
    }
}

[HarmonyPatch]
class LoadSceneAsyncNameIndexInternal__sceneName__sceneBuildIndex__parameters__mustCompleteNextFrame
{
    static MethodBase TargetMethod()
    {
        // string sceneName, int sceneBuildIndex, LoadSceneParameters parameters, bool mustCompleteNextFrame
        var loadSceneParametersType = AccessTools.TypeByName("UnityEngine.SceneManagement.LoadSceneParameters");
        return AccessTools.Method(Helper.GetSceneManager(), "LoadSceneAsyncNameIndexInternal", new Type[] { typeof(string), typeof(int), loadSceneParametersType, typeof(bool) });
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static bool Prefix(bool mustCompleteNextFrame, string sceneName, int sceneBuildIndex, object parameters, ref AsyncOperation __result)
    {
        if (!mustCompleteNextFrame)
        {
            Plugin.Log.LogDebug($"async scene load");
            __result = new AsyncOperation();
            GameTracker.AsyncSceneLoad(sceneName, sceneBuildIndex, parameters, ref __result);
            Plugin.Log.LogDebug($"setting up async scene load, assigned UID {new VersionSafeWrapper.AsyncOperationWrap(__result).UID}");
            return false;
        }
        return true;
    }
}