using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.UnitySafeWrappers.Interfaces;
using UniTASPlugin.UnitySafeWrappers.Interfaces.SceneManagement;
using UnityEngine;

namespace UniTASPlugin.UnitySafeWrappers.Wrappers;

// ReSharper disable once UnusedType.Global
public class SceneWrapper : ISceneWrapper
{
    private readonly ILoadSceneParametersWrapper _loadSceneParametersWrapper;
    private readonly ISceneWrap _sceneWrap;

    private const string SceneManagementNamespace = "UnityEngine.SceneManagement";

    private readonly Type _sceneManager = AccessTools.TypeByName($"{SceneManagementNamespace}.SceneManager");

    private readonly PropertyInfo _totalSceneCount;

    private readonly Type _loadSceneParametersType =
        AccessTools.TypeByName($"{SceneManagementNamespace}.LoadSceneParameters");

    private readonly Type _sceneManagerAPIInternal =
        AccessTools.TypeByName($"{SceneManagementNamespace}.SceneManagerAPIInternal");

    // load level async
    private readonly MethodInfo _loadSceneAsyncNameIndexInternalInjected;

    // fallback load level async 1
    private readonly MethodInfo _loadSceneAsyncNameIndexInternal;

    // fallback load level async 2
    private readonly MethodInfo _applicationLoadLevelAsync;

    // non-async load level
    private readonly MethodInfo _loadScene;

    private readonly MethodInfo _getActiveScene;

    public SceneWrapper(ILoadSceneParametersWrapper loadSceneParametersWrapper, ISceneWrap sceneWrap)
    {
        _loadSceneParametersWrapper = loadSceneParametersWrapper;
        _sceneWrap = sceneWrap;
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

        _loadSceneAsyncNameIndexInternalInjected = _sceneManagerAPIInternal?.GetMethod(
            "LoadSceneAsyncNameIndexInternal_Injected", AccessTools.all,
            null, new[] { typeof(string), typeof(int), _loadSceneParametersType?.MakeByRefType(), typeof(bool) }, null);

        _totalSceneCount = _sceneManager?.GetProperty("sceneCountInBuildSettings", AccessTools.all);

        _getActiveScene = _sceneManager?.GetMethod("GetActiveScene", AccessTools.all);
    }

    public void LoadSceneAsync(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, bool mustCompleteNextFrame)
    {
        if (_loadSceneAsyncNameIndexInternalInjected != null)
        {
            _loadSceneParametersWrapper.CreateInstance();
            _loadSceneParametersWrapper.LoadSceneMode = loadSceneMode;
            _loadSceneParametersWrapper.LocalPhysicsMode = localPhysicsMode;
            _loadSceneAsyncNameIndexInternalInjected.Invoke(null,
                new[] { sceneName, sceneBuildIndex, _loadSceneParametersWrapper.Instance, mustCompleteNextFrame });
            return;
        }

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

    public int TotalSceneCount
    {
        get
        {
            if (_totalSceneCount != null)
            {
                return (int)_totalSceneCount.GetValue(null, null);
            }

            return Application.levelCount;
        }
    }

    public int ActiveSceneIndex
    {
        get
        {
            if (_getActiveScene != null)
            {
                _sceneWrap.Instance = _getActiveScene.Invoke(null, null);
                return _sceneWrap.BuildIndex;
            }

            return Application.loadedLevel;
        }
    }

    public string ActiveSceneName
    {
        get
        {
            if (_getActiveScene != null)
            {
                _sceneWrap.Instance = _getActiveScene.Invoke(null, null);
                return _sceneWrap.Name;
            }

            return Application.loadedLevelName;
        }
    }
}