using System;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.UnitySafeWrappers;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
[ExcludeRegisterIfTesting]
public class SceneManagerWrapper : ISceneWrapper
{
    private readonly IUnityInstanceWrapFactory _unityInstanceWrapFactory;
    private readonly IPatchReverseInvoker _patchReverseInvoker;

    private const string SceneManagementNamespace = "UnityEngine.SceneManagement";

    private readonly Type _sceneManager = AccessTools.TypeByName($"{SceneManagementNamespace}.SceneManager");

    private readonly Func<int> _totalSceneCount;

    private readonly Type _loadSceneParametersType =
        AccessTools.TypeByName($"{SceneManagementNamespace}.LoadSceneParameters");

    private readonly Type _sceneManagerAPIInternal =
        AccessTools.TypeByName($"{SceneManagementNamespace}.SceneManagerAPIInternal");

    // load level async
    private readonly MethodInfo _loadSceneAsyncNameIndexInternalInjected;

    // fallback load level async 1
    private readonly MethodInfo _loadSceneAsyncNameIndexInternal;

    // fallback load level async 2
    private readonly Func<string, int, bool, bool, AsyncOperation> _applicationLoadLevelAsync;

    // non-async load level
    private readonly MethodInfo _loadSceneByIndex;
    private readonly MethodInfo _loadSceneByName;

    private readonly MethodInfo _getActiveScene;

    public SceneManagerWrapper(IUnityInstanceWrapFactory unityInstanceWrapFactory,
        IPatchReverseInvoker patchReverseInvoker)
    {
        _unityInstanceWrapFactory = unityInstanceWrapFactory;
        _patchReverseInvoker = patchReverseInvoker;
        const string loadSceneAsyncNameIndexInternal = "LoadSceneAsyncNameIndexInternal";
        _loadSceneAsyncNameIndexInternal = _sceneManager?.GetMethod(loadSceneAsyncNameIndexInternal, AccessTools.all,
            null, [typeof(string), typeof(int), typeof(bool), typeof(bool)], null);

        if (_loadSceneAsyncNameIndexInternal == null && _loadSceneParametersType != null)
        {
            _loadSceneAsyncNameIndexInternal = _sceneManager?.GetMethod(loadSceneAsyncNameIndexInternal,
                AccessTools.all,
                null,
                [typeof(string), typeof(int), _loadSceneParametersType, typeof(bool)], null);
        }

        _loadSceneByIndex = _sceneManager?.GetMethod("LoadScene", AccessTools.all, null, [typeof(int)], null);
        _loadSceneByName = _sceneManager?.GetMethod("LoadScene", AccessTools.all, null, [typeof(string)], null);

        var loadLevelAsync = AccessTools.Method(typeof(Application), "LoadLevelAsync",
            [typeof(string), typeof(int), typeof(bool), typeof(bool)]);
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
                null, [typeof(string), typeof(int), _loadSceneParametersType.MakeByRefType(), typeof(bool)],
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
            var instance = _unityInstanceWrapFactory.Create<LoadSceneParametersWrapper>(null);
            instance.LoadSceneMode = loadSceneMode;
            instance.LocalPhysicsMode = localPhysicsMode;
            _patchReverseInvoker.Invoke(
                (load, sceneNameInner, sceneBuildIndexInner, loadSceneParams, mustCompleteNextFrameInner) =>
                    load.Invoke(null,
                        [sceneNameInner, sceneBuildIndexInner, loadSceneParams, mustCompleteNextFrameInner]),
                _loadSceneAsyncNameIndexInternalInjected, sceneName, sceneBuildIndex, instance.Instance,
                mustCompleteNextFrame);
            return;
        }

        if (_loadSceneAsyncNameIndexInternal != null)
        {
            _patchReverseInvoker.Invoke(
                (load, sceneNameInner, sceneBuildIndexInner, additive, mustCompleteNextFrameInner) => load?.Invoke(null,
                    [sceneNameInner, sceneBuildIndexInner, additive, mustCompleteNextFrameInner]),
                _loadSceneAsyncNameIndexInternal, sceneName, sceneBuildIndex, loadSceneMode == LoadSceneMode.Additive,
                mustCompleteNextFrame);
            return;
        }

        if (_applicationLoadLevelAsync != null)
        {
            _patchReverseInvoker.Invoke(
                (load, sceneNameInner, sceneBuildIndexInner, additive, mustCompleteNextFrameInner) =>
                    load.Invoke(sceneNameInner, sceneBuildIndexInner, additive, mustCompleteNextFrameInner),
                _applicationLoadLevelAsync, sceneName, sceneBuildIndex, loadSceneMode == LoadSceneMode.Additive,
                mustCompleteNextFrame);
            return;
        }

        throw new InvalidOperationException("Could not find any more alternative load async methods");
    }

    public void LoadScene(int buildIndex)
    {
        if (_loadSceneByIndex != null)
        {
            _loadSceneByIndex.Invoke(null, [buildIndex]);
            return;
        }

        Application.LoadLevel(buildIndex);
    }

    public void LoadScene(string name)
    {
        if (_loadSceneByName != null)
        {
            _loadSceneByName.Invoke(null, [name]);
            return;
        }

        Application.LoadLevel(name);
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