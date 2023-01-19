using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.UnitySafeWrappers.Interfaces;
using UniTASPlugin.UnitySafeWrappers.Interfaces.SceneManagement;
using UnityEngine;

namespace UniTASPlugin.UnitySafeWrappers.Wrappers;

public class SceneWrapper : ISceneWrapper
{
    private readonly ILoadSceneParametersWrapper _loadSceneParametersWrapper;

    private readonly Type _sceneManager = AccessTools.TypeByName("UnityEngine.SceneManagement.SceneManager");

    private readonly Type _loadSceneParametersType =
        AccessTools.TypeByName("UnityEngine.SceneManagement.LoadSceneParameters");

    private readonly MethodInfo _loadSceneAsyncNameIndexInternal;

    // fallback load level async
    private readonly MethodInfo _applicationLoadLevelAsync;

    // non-async load level
    private readonly MethodInfo _loadScene;

    public SceneWrapper(ILoadSceneParametersWrapper loadSceneParametersWrapper)
    {
        _loadSceneParametersWrapper = loadSceneParametersWrapper;
        const string loadSceneAsyncNameIndexInternal = "LoadSceneAsyncNameIndexInternal";
        _loadSceneAsyncNameIndexInternal = _sceneManager?.GetMethod(loadSceneAsyncNameIndexInternal, AccessTools.all,
            null, new[] { typeof(string), typeof(int), typeof(bool), typeof(bool) }, null);

        if (_loadSceneAsyncNameIndexInternal == null && _loadSceneParametersType != null)
        {
            _loadSceneAsyncNameIndexInternal = _sceneManager?.GetMethod(loadSceneAsyncNameIndexInternal,
                AccessTools.all,
                null,
                new[] { typeof(string), typeof(int), _loadSceneParametersType, typeof(bool) }, null);
        }

        _loadScene = _sceneManager?.GetMethod("LoadScene", AccessTools.all, null, new[] { typeof(int) }, null);

        _applicationLoadLevelAsync = AccessTools.TypeByName("UnityEngine.Application")?.GetMethod("LoadLevelAsync",
            AccessTools.all, null, new[] { typeof(string), typeof(int), typeof(bool), typeof(bool) }, null);
    }

    public void LoadSceneAsync(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, bool mustCompleteNextFrame)
    {
        if (_loadSceneAsyncNameIndexInternal != null)
        {
            _loadSceneParametersWrapper.CreateInstance();
            _loadSceneParametersWrapper.LoadSceneMode = loadSceneMode;
            _loadSceneParametersWrapper.LocalPhysicsMode = localPhysicsMode;
            _loadSceneAsyncNameIndexInternal?.Invoke(null,
                new[] { sceneName, sceneBuildIndex, _loadSceneParametersWrapper.Instance, mustCompleteNextFrame });
            return;
        }

        if (_applicationLoadLevelAsync != null)
        {
            _applicationLoadLevelAsync.Invoke(null,
                new object[]
                    { sceneName, sceneBuildIndex, loadSceneMode == LoadSceneMode.Additive, mustCompleteNextFrame });
            return;
        }

        throw new InvalidOperationException("Could not find any more alternative load async methods");
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