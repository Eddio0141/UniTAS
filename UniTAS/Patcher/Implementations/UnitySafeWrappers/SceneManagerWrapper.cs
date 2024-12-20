using System;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.UnitySafeWrappers;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
[ExcludeRegisterIfTesting]
public class SceneManagerWrapper : ISceneManagerWrapper, IOnPreGameRestart
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

    private readonly MethodInfo _unloadSceneNameIndexInternal;
    private readonly bool _unloadSceneNameIndexInternalHasOptions;

    private readonly MethodInfo _getActiveScene;
    private readonly MethodInfo _getSceneAt;
    private readonly Func<int> _sceneCount;

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
        _getSceneAt = _sceneManager?.GetMethod("GetSceneAt", AccessTools.all, null, [typeof(int)], null);
        _sceneCount = _sceneManager?.GetProperty("sceneCount", AccessTools.all)?.GetGetMethod()
            ?.MethodDelegate<Func<int>>();

        var unloadSceneOptions = AccessTools.TypeByName($"{SceneManagementNamespace}.UnloadSceneOptions");
        _unloadSceneNameIndexInternal = unloadSceneOptions == null
            ? null
            : AccessTools.Method(_sceneManagerAPIInternal ?? _sceneManager,
                  "UnloadSceneNameIndexInternal",
                  [typeof(string), typeof(int), typeof(bool), unloadSceneOptions, typeof(bool).MakeByRefType()]) ??
              AccessTools.Method(_sceneManagerAPIInternal ?? _sceneManager,
                  "UnloadSceneNameIndexInternal",
                  [typeof(string), typeof(int), typeof(bool), typeof(bool).MakeByRefType()]);

        if (unloadSceneOptions != null)
        {
            _unloadSceneNameIndexInternal = AccessTools.Method(_sceneManagerAPIInternal ?? _sceneManager,
                "UnloadSceneNameIndexInternal",
                [typeof(string), typeof(int), typeof(bool), unloadSceneOptions, typeof(bool).MakeByRefType()]);
            _unloadSceneNameIndexInternalHasOptions = _unloadSceneNameIndexInternal != null;
        }

        _unloadSceneNameIndexInternal ??= AccessTools.Method(_sceneManagerAPIInternal ?? _sceneManager,
            "UnloadSceneNameIndexInternal",
            [typeof(string), typeof(int), typeof(bool), typeof(bool).MakeByRefType()]);
    }

    public void OnPreGameRestart()
    {
        LoadedSceneCountDummy = 1;
    }

    public void LoadSceneAsync(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, bool mustCompleteNextFrame)
    {
        if (TrackSceneCountDummy)
        {
            if (loadSceneMode == LoadSceneMode.Additive)
                LoadedSceneCountDummy++;
            else
                LoadedSceneCountDummy = 1;
        }

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
        if (TrackSceneCountDummy)
            LoadedSceneCountDummy = 1;

        if (_loadSceneByIndex != null)
        {
            _patchReverseInvoker.Invoke((load, index) => load.Invoke(null, [index]), _loadSceneByIndex, buildIndex);
            return;
        }

        _patchReverseInvoker.Invoke(Application.LoadLevel, buildIndex);
    }

    public void LoadScene(string name)
    {
        if (TrackSceneCountDummy)
            LoadedSceneCountDummy = 1;

        if (_loadSceneByName != null)
        {
            _patchReverseInvoker.Invoke((load, n) => load.Invoke(null, [n]), _loadSceneByName, name);
            return;
        }

        _patchReverseInvoker.Invoke(Application.LoadLevel, name);
    }

    public void UnloadSceneAsync(string sceneName, int sceneBuildIndex, object options, bool immediate,
        out bool success)
    {
        object[] args;
        if (_unloadSceneNameIndexInternalHasOptions)
        {
            args = [sceneName, sceneBuildIndex, immediate, options, null];
        }
        else
        {
            args = [sceneName, sceneBuildIndex, immediate, null];
        }

        var op = _patchReverseInvoker.Invoke((m, a) => m.Invoke(null, a), _unloadSceneNameIndexInternal, args);
        success = (bool)args[args.Length - 1];

        if (TrackSceneCountDummy && (!immediate || op != null) && success)
            LoadedSceneCountDummy = Math.Max(1, LoadedSceneCountDummy - 1);
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
                return sceneWrapInstance.BuildIndex;
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

    public int LoadedSceneCountDummy { get; set; } = 1;
    public bool TrackSceneCountDummy { get; set; }

    public SceneWrapper GetSceneAt(int index)
    {
        return _getSceneAt == null ? null : new SceneWrapper(_getSceneAt.Invoke(null, [index]));
    }

    public int SceneCount
    {
        get
        {
            if (_sceneCount == null) return -1;
            return _patchReverseInvoker.Invoke(call => call(), _sceneCount);
        }
    }
}