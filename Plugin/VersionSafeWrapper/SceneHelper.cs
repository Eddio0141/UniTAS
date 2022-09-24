using HarmonyLib;
using System;
using System.Reflection;

namespace UniTASPlugin.VersionSafeWrapper;

internal static class SceneHelper
{
    static Type SceneManager()
    {
        return AccessTools.TypeByName("UnityEngine.SceneManagement.SceneManager");
    }

    static Type loadSceneParametersType()
    {
        return AccessTools.TypeByName("UnityEngine.SceneManagement.LoadSceneParameters");
    }

    static MethodInfo LoadSceneAsyncNameIndexInternal()
    {
        var sceneManagerType = SceneManager();
        var methodName = "LoadSceneAsyncNameIndexInternal";
        var method = sceneManagerType.GetMethod(methodName, AccessTools.all, null, new Type[] { typeof(string), typeof(int), typeof(bool), typeof(bool) }, null);
        if (method != null)
            return method;
        var loadSceneParameters = loadSceneParametersType();
        if (loadSceneParameters != null)
            return sceneManagerType.GetMethod(methodName, AccessTools.all, null, new Type[] { typeof(string), typeof(int), loadSceneParameters, typeof(bool) }, null);
        else
            return null;
    }

    public static void LoadScene(int buildIndex)
    {
        var sceneManager = SceneManager();
        if (sceneManager != null)
        {
            var loadScene = AccessTools.Method(sceneManager, "LoadScene", new Type[] { typeof(int) });
            if (loadScene != null)
            {
                loadScene.Invoke(null, new object[] { buildIndex });
                return;
            }
        }
        Plugin.Log.LogError("Failed to load scene by build index, missing method");
    }

    public static void LoadSceneAsyncNameIndexInternal(string sceneName, int sceneBuildIndex, object parameters, bool? isAdditive, bool mustCompleteNextFrame)
    {
        var loader = LoadSceneAsyncNameIndexInternal();
        if (loader == null)
        {
            Plugin.Log.LogError("Load scene async doesn't exist in this version of unity");
            return;
        }
        if (isAdditive.HasValue)
            loader.Invoke(null, new object[] { sceneName, sceneBuildIndex, (bool)isAdditive, mustCompleteNextFrame });
        else
            loader.Invoke(null, new object[] { sceneName, sceneBuildIndex, parameters, mustCompleteNextFrame });
    }
}
