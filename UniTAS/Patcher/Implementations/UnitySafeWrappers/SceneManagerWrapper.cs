using System;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Services.UnitySafeWrappers;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class SceneManagerWrapper : ISceneWrapper
{
    private readonly IUnityInstanceWrapFactory _unityInstanceWrapFactory;

    private const string SCENE_MANAGEMENT_NAMESPACE = "UnityEngine.SceneManagement";

    private readonly Type _sceneManager = AccessTools.TypeByName($"{SCENE_MANAGEMENT_NAMESPACE}.SceneManager");

    private readonly Func<int> _totalSceneCount;

    private readonly Type _loadSceneParametersType =
        AccessTools.TypeByName($"{SCENE_MANAGEMENT_NAMESPACE}.LoadSceneParameters");

    private readonly Type _sceneManagerAPIInternal =
        AccessTools.TypeByName($"{SCENE_MANAGEMENT_NAMESPACE}.SceneManagerAPIInternal");

    // load level async
    private readonly MethodInfo _loadSceneAsyncNameIndexInternalInjected;

    // fallback load level async 1
    private readonly MethodInfo _loadSceneAsyncNameIndexInternal;

    // fallback load level async 2
    private readonly Func<string, int, bool, bool, AsyncOperation> _applicationLoadLevelAsync;

    // non-async load level
    private readonly MethodInfo _loadScene;

    private readonly MethodInfo _getActiveScene;

    public SceneManagerWrapper(IUnityInstanceWrapFactory unityInstanceWrapFactory)
    {
        _unityInstanceWrapFactory = unityInstanceWrapFactory;
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

        var loadLevelAsync = AccessTools.Method("UnityEngine.Application.LoadLevelAsync",
            new[] { typeof(string), typeof(int), typeof(bool), typeof(bool) });
        if (loadLevelAsync != null)
        {
            _applicationLoadLevelAsync =
                AccessTools.MethodDelegate<Func<string, int, bool, bool, AsyncOperation>>(loadLevelAsync);
        }

        if (_loadSceneParametersType != null)
        {
            var usingType = _sceneManagerAPIInternal ?? _sceneManager;
            _loadSceneAsyncNameIndexInternalInjected = usingType?.GetMethod(
                "LoadSceneAsyncNameIndexInternal_Injected", AccessTools.all,
                null, new[] { typeof(string), typeof(int), _loadSceneParametersType.MakeByRefType(), typeof(bool) },
                null);
        }

        if (_sceneManager != null)
        {
            _totalSceneCount =
                AccessTools.MethodDelegate<Func<int>>(AccessTools.PropertyGetter(_sceneManager,
                    "sceneCountInBuildSettings"));
        }

        _getActiveScene = _sceneManager?.GetMethod("GetActiveScene", AccessTools.all);
    }

    public void LoadSceneAsync(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, bool mustCompleteNextFrame)
    {
        if (_loadSceneAsyncNameIndexInternalInjected != null && _loadSceneParametersType != null)
        {
            var instance = _unityInstanceWrapFactory.CreateNew<LoadSceneParametersWrapper>();
            instance.LoadSceneMode = loadSceneMode;
            instance.LocalPhysicsMode = localPhysicsMode;
            _loadSceneAsyncNameIndexInternalInjected.Invoke(null,
                new[] { sceneName, sceneBuildIndex, instance.Instance, mustCompleteNextFrame });
            return;
        }

        if (_loadSceneAsyncNameIndexInternal != null)
        {
            _loadSceneAsyncNameIndexInternal?.Invoke(null,
                new object[]
                    { sceneName, sceneBuildIndex, loadSceneMode == LoadSceneMode.Additive, mustCompleteNextFrame });
            return;
        }

        if (_applicationLoadLevelAsync != null)
        {
            _applicationLoadLevelAsync.Invoke(sceneName, sceneBuildIndex, loadSceneMode == LoadSceneMode.Additive,
                mustCompleteNextFrame);
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

    public int TotalSceneCount => _totalSceneCount?.Invoke() ?? Application.levelCount;

    public int ActiveSceneIndex
    {
        get
        {
            if (_getActiveScene != null)
            {
                var sceneWrapInstance =
                    _unityInstanceWrapFactory.Create<SceneWrapper>(_getActiveScene.Invoke(null, null));
                return sceneWrapInstance.BuildIndex ?? Application.loadedLevel;
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
                var sceneWrapInstance =
                    _unityInstanceWrapFactory.Create<SceneWrapper>(_getActiveScene.Invoke(null, null));
                return sceneWrapInstance.Name;
            }

            return Application.loadedLevelName;
        }
    }
}