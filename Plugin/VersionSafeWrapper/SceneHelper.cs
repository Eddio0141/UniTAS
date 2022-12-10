using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.VersionSafeWrapper;

internal static class SceneHelper
{
    private static readonly Type SceneManager = AccessTools.TypeByName("UnityEngine.SceneManagement.SceneManager");

    private static readonly Type LoadSceneParametersType =
        AccessTools.TypeByName("UnityEngine.SceneManagement.LoadSceneParameters");

    private static MethodInfo LoadSceneAsyncNameIndexInternal()
    {
        const string methodName = "LoadSceneAsyncNameIndexInternal";
        var method = SceneManager.GetMethod(methodName, AccessTools.all, null,
            new[] { typeof(string), typeof(int), typeof(bool), typeof(bool) }, null);
        return method ?? (LoadSceneParametersType != null
            ? SceneManager.GetMethod(methodName, AccessTools.all, null,
                new[] { typeof(string), typeof(int), LoadSceneParametersType, typeof(bool) }, null)
            : null);
    }

    public static void LoadScene(int buildIndex)
    {
        if (SceneManager != null)
        {
            var loadScene = AccessTools.Method(SceneManager, "LoadScene", new[] { typeof(int) });
            if (loadScene != null)
            {
                _ = loadScene.Invoke(null, new object[] { buildIndex });
                return;
            }
        }

        Application.LoadLevel(0);
    }

    public static void LoadSceneAsyncNameIndexInternal(string sceneName, int sceneBuildIndex, object parameters,
        bool? isAdditive, bool mustCompleteNextFrame)
    {
        var loader = LoadSceneAsyncNameIndexInternal();
        if (loader == null)
        {
            Plugin.Log.LogError("Load scene async doesn't exist in this version of unity");
            return;
        }

        _ = isAdditive.HasValue
            ? loader.Invoke(null, new object[] { sceneName, sceneBuildIndex, (bool)isAdditive, mustCompleteNextFrame })
            : loader.Invoke(null, new[] { sceneName, sceneBuildIndex, parameters, mustCompleteNextFrame });
    }
}