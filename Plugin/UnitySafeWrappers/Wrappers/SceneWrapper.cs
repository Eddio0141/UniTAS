using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.UnitySafeWrappers.Interfaces;
using UnityEngine;

namespace UniTASPlugin.UnitySafeWrappers.Wrappers;

public class SceneWrapper : ISceneWrapper
{
    private readonly Type _sceneManager = AccessTools.TypeByName("UnityEngine.SceneManagement.SceneManager");

    private readonly Type _loadSceneParametersType =
        AccessTools.TypeByName("UnityEngine.SceneManagement.LoadSceneParameters");

    private readonly MethodInfo _loadSceneAsyncNameIndexInternal;
    private readonly MethodInfo _loadScene;

    public SceneWrapper()
    {
        const string loadSceneAsyncNameIndexInternal = "LoadSceneAsyncNameIndexInternal";
        _loadSceneAsyncNameIndexInternal = _sceneManager.GetMethod(loadSceneAsyncNameIndexInternal, AccessTools.all,
            null, new[] { typeof(string), typeof(int), typeof(bool), typeof(bool) }, null);

        if (_loadSceneAsyncNameIndexInternal == null)
        {
            if (_loadSceneParametersType == null)
            {
                throw new InvalidOperationException("Could not find LoadSceneAsyncNameIndexInternal method");
            }

            _loadSceneAsyncNameIndexInternal = _sceneManager.GetMethod(loadSceneAsyncNameIndexInternal, AccessTools.all,
                null,
                new[] { typeof(string), typeof(int), _loadSceneParametersType, typeof(bool) }, null);
        }

        _loadScene = _sceneManager.GetMethod("LoadScene", AccessTools.all, null, new[] { typeof(int) }, null);
    }

    public void LoadSceneAsync(string sceneName, int sceneBuildIndex, object parameters, bool? isAdditive,
        bool mustCompleteNextFrame)
    {
        _loadSceneAsyncNameIndexInternal.Invoke(null,
            isAdditive.HasValue
                ? new object[] { sceneName, sceneBuildIndex, (bool)isAdditive, mustCompleteNextFrame }
                : new[] { sceneName, sceneBuildIndex, parameters, mustCompleteNextFrame });
    }

    public void LoadScene(int buildIndex)
    {
        if (_loadScene != null)
        {
            _loadScene.Invoke(null, new object[] { buildIndex });
            return;
        }

        Application.LoadLevel(0);
    }
}