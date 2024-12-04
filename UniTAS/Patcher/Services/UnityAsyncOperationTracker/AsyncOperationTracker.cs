using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Extensions;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Models.Utils;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnityInfo;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class AsyncOperationTracker : ISceneLoadTracker, IAssetBundleCreateRequestTracker, IAssetBundleRequestTracker,
    IOnLastUpdateUnconditional, IAsyncOperationIsInvokingOnComplete, IOnPreGameRestart, IOnUpdateActual,
    IOnStartActual, IOnFixedUpdateActual, IAssetBundleTracker
{
    private readonly HashSet<AsyncOperation> _tracked = [];

    // any scene related things are stored as a list
    // it is a list of scenes in load order
    private readonly List<AsyncSceneLoadData> _asyncLoads = [];

    // not allowed to be disabled anymore
    private readonly HashSet<AsyncOperation> _allowSceneActivationNotAllowed = [];

    // this can have unload operations too, which is why its Either
    private readonly List<Either<AsyncSceneLoadData, AsyncSceneUnloadData>> _pendingLoadCallbacks = [];
    private readonly List<AsyncSceneLoadData> _asyncLoadStalls = [];

    private readonly Dictionary<AsyncOperation, AssetBundle> _assetBundleCreateRequests = new();
    private readonly Dictionary<AsyncOperation, AssetBundleRequestData> _assetBundleRequests = new();
    private readonly Dictionary<AssetBundle, string[]> _bundleScenes = new();

    private readonly Dictionary<AsyncOperation, bool> _allowSceneActivationValue = new();

    private readonly HashSet<AsyncOperation> _isDone = [];

    private bool _sceneLoadSync;

    private readonly Func<AssetBundle, string[]> _getAllScenePaths;

    private class AssetBundleRequestData(Object singleResult = null, Object[] multipleResults = null)
    {
        public Object SingleResult { get; } = singleResult;
        public Object[] MultipleResults { get; } = multipleResults;
    }

    public void OnPreGameRestart()
    {
        _asyncLoads.Clear();
        _pendingLoadCallbacks.Clear();
        _asyncLoadStalls.Clear();
        _assetBundleCreateRequests.Clear();
        _assetBundleRequests.Clear();
        _allowSceneActivationValue.Clear();
        _isDone.Clear();
        _tracked.Clear();
        _allowSceneActivationNotAllowed.Clear();
        _bundleScenes.Clear();
        LoadingScenes.Clear();
        LoadingSceneCount = 0;
        _sceneLoadSync = false;
    }

    public void OnLastUpdateUnconditional()
    {
        if (_asyncLoads.Count == 0) return;

        var removes = new List<int>();
        for (var i = 0; i < _asyncLoads.Count; i++)
        {
            var loadData = _asyncLoads[i];
            if (loadData.DelayFrame) continue;
            removes.Add(i);

            _logger.LogDebug(
                $"force loading scene via OnLastUpdate, name: {loadData.SceneName}, index: {loadData.SceneBuildIndex}, manually loading in loop");
            _iSceneManagerWrapper.TrackSceneCountDummy = false;
            _iSceneManagerWrapper.LoadSceneAsync(loadData.SceneName, loadData.SceneBuildIndex, loadData.LoadSceneMode,
                loadData.LocalPhysicsMode, true);
            _iSceneManagerWrapper.TrackSceneCountDummy = true;

            // call after scene is loaded
            ReplaceDummyScene();

            _pendingLoadCallbacks.Add(loadData);
            var op = loadData.AsyncOperation;
            if (op == null) continue;
            _allowSceneActivationNotAllowed.Remove(op);
        }

        foreach (var i in ((IEnumerable<int>)removes).Reverse())
        {
            _asyncLoads.RemoveAt(i);
        }
    }

    public void FixedUpdateActual() => CallPendingCallbacks();

    public void UpdateActual()
    {
        _sceneLoadSync = false;

        // doesn't matter, as long as next frame happens, before the game update adds more scenes, delay is gone
        foreach (var load in _asyncLoads.Concat(_asyncLoadStalls))
        {
            load.DelayFrame = false;
        }

        CallPendingCallbacks();
    }

    public void StartActual() => CallPendingCallbacks();

    private void CallPendingCallbacks()
    {
        if (_pendingLoadCallbacks.Count == 0) return;

        // to allow the scene to be findable, invoke when scene loads on update
        var callbacks = new List<Either<AsyncSceneLoadData, AsyncSceneUnloadData>>(_pendingLoadCallbacks.Count);
        foreach (var data in _pendingLoadCallbacks)
        {
            callbacks.Add(data);
            var op = data.IsLeft ? data.Left.AsyncOperation : data.Right.AsyncOperation;
            if (op == null) continue;
            _isDone.Add(op);
        }

        // prevent allowSceneActivation to be False on event callback
        _pendingLoadCallbacks.Clear();

        foreach (var data in callbacks)
        {
            LoadingSceneCount--;
            AsyncOperation op;
            if (data.IsLeft)
            {
                var left = data.Left;
                if (left.LoadSceneMode == LoadSceneMode.Additive)
                    _iSceneManagerWrapper.SceneCountDummy++;
                else
                    _iSceneManagerWrapper.SceneCountDummy = 1;

                op = left.AsyncOperation;
            }
            else
            {
                op = data.Right.AsyncOperation;
            }

            if (op == null) continue;
            InvokeOnComplete(op);
        }
    }

    public void NewAssetBundleRequest(AsyncOperation asyncOperation, Object assetBundleRequest)
    {
        _assetBundleRequests.Add(asyncOperation, new(assetBundleRequest));
        _tracked.Add(asyncOperation);
        _isDone.Add(asyncOperation);
        InvokeOnComplete(asyncOperation);
    }

    public void NewAssetBundleRequestMultiple(AsyncOperation asyncOperation, Object[] assetBundleRequestArray)
    {
        _assetBundleRequests.Add(asyncOperation, new(multipleResults: assetBundleRequestArray));
        _tracked.Add(asyncOperation);
        _isDone.Add(asyncOperation);
        InvokeOnComplete(asyncOperation);
    }

    public void NewAssetBundleCreateRequest(AsyncOperation asyncOperation, AssetBundle assetBundle)
    {
        _assetBundleCreateRequests.Add(asyncOperation, assetBundle);
        if (_getAllScenePaths != null)
            _bundleScenes[assetBundle] = _getAllScenePaths(assetBundle);
        _tracked.Add(asyncOperation);
        _isDone.Add(asyncOperation);
        InvokeOnComplete(asyncOperation);
    }

    public void AsyncSceneUnload(ref AsyncOperation asyncOperation, string sceneName)
    {
        _logger.LogDebug($"async scene unload, name: {sceneName}");

        _tracked.Add(asyncOperation);
        if (_pendingLoadCallbacks.Any(p => p.IsRight && p.Right.SceneName == sceneName))
        {
            _logger.LogDebug("scene is already to be unloaded, skipping this operation");
            asyncOperation = null;
            return;
        }

        _pendingLoadCallbacks.Add(new AsyncSceneUnloadData(sceneName, asyncOperation));
        _iSceneManagerWrapper.SceneCountDummy--;
        LoadingSceneCount++;
    }

    public void AsyncSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, AsyncOperation asyncOperation)
    {
        LoadingSceneCount++;
        CreateDummySceneStruct(sceneName);

        _tracked.Add(asyncOperation);

        _logger.LogDebug($"async scene load, name: `{sceneName}`, index: {sceneBuildIndex}");
        var loadData =
            new AsyncSceneLoadData(sceneName, sceneBuildIndex, loadSceneMode, localPhysicsMode, asyncOperation);
        _asyncLoads.Add(loadData);

        if (!_sceneLoadSync) return;
        _logger.LogDebug("load scene sync is happening for next frame");

        // sync load is going to happen, make this load too
        loadData.DelayFrame = false;
        _allowSceneActivationNotAllowed.Add(asyncOperation);
    }

    public void NonAsyncSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode)
    {
        _sceneLoadSync = true;
        LoadingSceneCount++;

        // first, all stalls are moved to load
        _asyncLoads.AddRange(_asyncLoadStalls);
        foreach (var sceneData in _asyncLoads)
        {
            // delays are all gone now
            sceneData.DelayFrame = false;
            _logger.LogDebug(
                $"force loading scene via non-async scene load, name: {sceneData.SceneName}, index: {sceneData.SceneBuildIndex}");
            _allowSceneActivationNotAllowed.Add(sceneData.AsyncOperation);
        }

        _asyncLoadStalls.Clear();

        // next, queue the non async scene load
        _logger.LogDebug($"scene load, {sceneName}, index: {sceneBuildIndex}");
        _asyncLoads.Add(new(sceneName, sceneBuildIndex, loadSceneMode, localPhysicsMode, null));
    }

    public void AllowSceneActivation(bool allow, AsyncOperation asyncOperation)
    {
        _logger.LogDebug($"allow scene activation {allow}, {new StackTrace()}");

        if (allow)
        {
            var stallIndex = _asyncLoadStalls.FindIndex(d => d.AsyncOperation == asyncOperation);
            if (stallIndex < 0) return;
            StoreAllowSceneActivation(asyncOperation, true);
            if (_allowSceneActivationNotAllowed.Contains(asyncOperation)) return;
            _asyncLoads.Add(_asyncLoadStalls[stallIndex]);
            _asyncLoadStalls.RemoveAt(stallIndex);
            _logger.LogDebug("restored scene activation");
        }
        else
        {
            var loadIndex = _asyncLoads.FindIndex(d => d.AsyncOperation == asyncOperation);
            if (loadIndex < 0) return;
            StoreAllowSceneActivation(asyncOperation, false);
            if (_allowSceneActivationNotAllowed.Contains(asyncOperation)) return;
            _asyncLoadStalls.Add(_asyncLoads[loadIndex]);
            _asyncLoads.RemoveAt(loadIndex);
            _logger.LogDebug("Added scene to stall list");
        }
    }

    private void StoreAllowSceneActivation(AsyncOperation asyncOperation, bool allow)
    {
        _allowSceneActivationValue[asyncOperation] = allow;
    }

    public bool GetAllowSceneActivation(AsyncOperation asyncOperation, out bool state)
    {
        if (_allowSceneActivationValue.TryGetValue(asyncOperation, out state))
        {
            return true;
        }

        // not found, is it even tracked?
        state = false;
        return _tracked.Contains(asyncOperation);
    }

    public bool IsDone(AsyncOperation asyncOperation, out bool isDone)
    {
        if (_isDone.Contains(asyncOperation))
        {
            isDone = true;
            return true;
        }

        isDone = false;
        return _tracked.Contains(asyncOperation);
    }

    public bool Progress(AsyncOperation asyncOperation, out float progress)
    {
        if (_isDone.Contains(asyncOperation))
        {
            progress = 1f;
            return true;
        }

        progress = 0.9f;
        return _tracked.Contains(asyncOperation);
    }

    public AssetBundle GetAssetBundleCreateRequest(AsyncOperation asyncOperation)
    {
        return _assetBundleCreateRequests.TryGetValue(asyncOperation, out var assetBundle)
            ? assetBundle
            : null;
    }

    public object GetAssetBundleRequest(AsyncOperation asyncOperation)
    {
        return _assetBundleRequests.TryGetValue(asyncOperation, out var assetBundleRequest)
            ? assetBundleRequest.SingleResult
            : null;
    }

    public object GetAssetBundleRequestMultiple(AsyncOperation asyncOperation)
    {
        return _assetBundleRequests.TryGetValue(asyncOperation, out var assetBundleRequest)
            ? assetBundleRequest.MultipleResults
            : null;
    }

    private readonly MethodBase _invokeCompletionEvent =
        AccessTools.Method("UnityEngine.AsyncOperation:InvokeCompletionEvent", Type.EmptyTypes);

    private bool _isInvokingOnComplete;
    private readonly ISceneManagerWrapper _iSceneManagerWrapper;
    private readonly ILogger _logger;
    private readonly IGameBuildScenesInfo _gameBuildScenesInfo;

    public AsyncOperationTracker(ISceneManagerWrapper iSceneManagerWrapper, ILogger logger,
        IGameBuildScenesInfo gameBuildScenesInfo)
    {
        _iSceneManagerWrapper = iSceneManagerWrapper;
        _logger = logger;
        _gameBuildScenesInfo = gameBuildScenesInfo;
        var getAllScenePaths = AccessTools.Method(typeof(AssetBundle), "GetAllScenePaths");
        _getAllScenePaths = getAllScenePaths?.MethodDelegate<Func<AssetBundle, string[]>>();
    }

    public bool IsInvokingOnComplete(AsyncOperation asyncOperation, out bool wasInvoked)
    {
        if (_tracked.Contains(asyncOperation))
        {
            wasInvoked = _isInvokingOnComplete;
            return true;
        }

        wasInvoked = default;
        return false;
    }

    private void InvokeOnComplete(AsyncOperation asyncOperation)
    {
        if (_invokeCompletionEvent == null) return;
        _isInvokingOnComplete = true;
        _invokeCompletionEvent.Invoke(asyncOperation, null);
        _isInvokingOnComplete = false;
    }

    // note: this property exists here, because this class handles async scene loading, not scene wrapper
    public int LoadingSceneCount { get; private set; }

    public List<(object dummySceneStruct, IntPtr dummyScenePtr, LoadingScene loadingScene, SceneWrapper
            actualSceneStruct)>
        LoadingScenes { get; } = new();

    public void Unload(AssetBundle assetBundle)
    {
        _bundleScenes.Remove(assetBundle);
    }

    public void UnloadAsync(AsyncOperation op)
    {
        _tracked.Add(op);
        _isDone.Add(op);
        InvokeOnComplete(op);
    }

    private readonly Type _sceneStruct = AccessTools.TypeByName("UnityEngine.SceneManagement.Scene");

    private void CreateDummySceneStruct(string loadName)
    {
        if (_sceneStruct == null) return;
        var instance = Activator.CreateInstance(_sceneStruct);

        // find info about scene
        LoadingScene loadingScene;

        if (_gameBuildScenesInfo.NameToPath.TryGetValue(loadName, out var path))
        {
            // loadName is name
            loadingScene = new(path, loadName, _gameBuildScenesInfo.PathToIndex[path]);
        }
        else if (_gameBuildScenesInfo.PathToIndex.TryGetValue(loadName, out var index2))
        {
            // loadName is path
            loadingScene = new(loadName, _gameBuildScenesInfo.PathToName[loadName], index2);
        }
        // TODO: in AssetBundle scenes, can i load from just name, or do i need path
        // bundleScenes contains paths, not names
        else if (_bundleScenes.Values.Any(scenes => scenes.Any(scene => scene == loadName)))
        {
            // TODO: fix below after resolving above
            loadingScene = new(loadName, loadName, -1);
        }
        else
        {
            if (_getAllScenePaths == null)
            {
                _logger.LogError(
                    "A load was done but scene wasn't found, this may be wrong as " +
                    "AssetBundle.GetAllScenePaths doesn't exist in this unity version");
            }

            return;
        }

        var tr = __makeref(instance);
        IntPtr instancePtr;
        unsafe
        {
            instancePtr = *(IntPtr*)(&tr);
        }

        LoadingScenes.Add((instance, instancePtr, loadingScene, null));
    }

    private void ReplaceDummyScene()
    {
        var sceneCount = _iSceneManagerWrapper.SceneCount;
        if (sceneCount < 0) return;
        if (LoadingScenes.Count == 0) return;

        var actualScene = _iSceneManagerWrapper.GetSceneAt(sceneCount - 1);
        var loadingInfo = LoadingScenes[LoadingScenes.Count - 1];
        LoadingScenes[LoadingScenes.Count - 1] = (loadingInfo.dummySceneStruct, loadingInfo.dummyScenePtr,
            loadingInfo.loadingScene, actualScene);
    }

    private class AsyncSceneLoadData(
        string sceneName,
        int sceneBuildIndex,
        LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode,
        AsyncOperation asyncOperation)
    {
        public string SceneName { get; } = sceneName;
        public int SceneBuildIndex { get; } = sceneBuildIndex;
        public LoadSceneMode LoadSceneMode { get; } = loadSceneMode;
        public LocalPhysicsMode LocalPhysicsMode { get; } = localPhysicsMode;
        public AsyncOperation AsyncOperation { get; } = asyncOperation;
        public bool DelayFrame = true;
    }

    private class AsyncSceneUnloadData(string sceneName, AsyncOperation asyncOperation)
    {
        public string SceneName { get; } = sceneName;
        public AsyncOperation AsyncOperation { get; } = asyncOperation;
    }

    public class LoadingScene(string path, string name, int buildIndex)
    {
        public string Path { get; } = path;
        public string Name { get; } = name;
        public int BuildIndex { get; } = buildIndex;
    }
}