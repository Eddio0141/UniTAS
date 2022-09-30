using HarmonyLib;
using System;
using System.Reflection;

namespace UniTASPlugin.VersionSafeWrapper;

internal static class SceneHelper
{
    static readonly Type SceneManager = AccessTools.TypeByName("UnityEngine.SceneManagement.SceneManager");

    static readonly Type LoadSceneParametersType = AccessTools.TypeByName("UnityEngine.SceneManagement.LoadSceneParameters");

    static MethodInfo LoadSceneAsyncNameIndexInternal()
    {
        var methodName = "LoadSceneAsyncNameIndexInternal";
        var method = SceneManager.GetMethod(methodName, AccessTools.all, null, new Type[] { typeof(string), typeof(int), typeof(bool), typeof(bool) }, null);
        return method ?? (LoadSceneParametersType != null
            ? SceneManager.GetMethod(methodName, AccessTools.all, null, new Type[] { typeof(string), typeof(int), LoadSceneParametersType, typeof(bool) }, null)
            : null);
    }

    public static void LoadScene(int buildIndex)
    {
        if (SceneManager != null)
        {
            var loadScene = AccessTools.Method(SceneManager, "LoadScene", new Type[] { typeof(int) });
            if (loadScene != null)
            {
                _ = loadScene.Invoke(null, new object[] { buildIndex });
                return;
            }
        }
        UnityEngine.Application.LoadLevel(0);
    }

    public static void LoadSceneAsyncNameIndexInternal(string sceneName, int sceneBuildIndex, object parameters, bool? isAdditive, bool mustCompleteNextFrame)
    {
        var loader = LoadSceneAsyncNameIndexInternal();
        if (loader == null)
        {
            Plugin.Log.LogError("Load scene async doesn't exist in this version of unity");
            return;
        }
        _ = isAdditive.HasValue
            ? loader.Invoke(null, new object[] { sceneName, sceneBuildIndex, (bool)isAdditive, mustCompleteNextFrame })
            : loader.Invoke(null, new object[] { sceneName, sceneBuildIndex, parameters, mustCompleteNextFrame });
    }
}
