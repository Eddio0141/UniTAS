using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Events.UnityEvents;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class AsyncOperationTracker(ISceneWrapper sceneWrapper, ILogger logger, IOnSceneLoad[] onSceneLoads)
    : ISceneLoadTracker, IAssetBundleCreateRequestTracker, IAssetBundleRequestTracker,
        IOnLastUpdateUnconditional, IAsyncOperationIsInvokingOnComplete, IOnPreGameRestart, IOnUpdateActual
{
    private readonly List<AsyncOperation> _tracked = new();

    private readonly List<AsyncSceneLoadData> _asyncLoads = new();
    private readonly List<AsyncOperation> _pendingLoadCallbacks = new();
    private readonly List<AsyncSceneLoadData> _asyncLoadStalls = new();
    private readonly Dictionary<AsyncOperation, AssetBundle> _assetBundleCreateRequests = new();
    private readonly Dictionary<AsyncOperation, AssetBundleRequestData> _assetBundleRequests = new();
    private readonly List<(bool, AsyncOperation)> _allowSceneActivationValue = new();
    private readonly List<AsyncOperation> _isDone = new();

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
    }

    public void OnLastUpdateUnconditional()
    {
        foreach (var scene in _asyncLoads)
        {
            logger.LogDebug(
                $"force loading scene, name: {scene.SceneName}, index: {scene.SceneBuildIndex}, manually loading in loop");
            sceneWrapper.LoadSceneAsync(scene.SceneName, scene.SceneBuildIndex, scene.LoadSceneMode,
                scene.LocalPhysicsMode, true);

            _pendingLoadCallbacks.Add(scene.AsyncOperationInstance);
        }

        _asyncLoads.Clear();
    }

    public void UpdateActual()
    {
        // to allow the scene to be findable, invoke when scene loads on update
        var callbacks = new List<AsyncOperation>(_pendingLoadCallbacks.Count);
        foreach (var pendingLoadCallback in _pendingLoadCallbacks)
        {
            callbacks.Add(pendingLoadCallback);
            _isDone.Add(pendingLoadCallback);
        }

        // prevent allowSceneActivation to be False on event callback
        _pendingLoadCallbacks.Clear();

        foreach (var callback in callbacks)
        {
            // event fired
            InvokeOnComplete(callback);
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
        _tracked.Add(asyncOperation);
        _isDone.Add(asyncOperation);
        InvokeOnComplete(asyncOperation);
    }

    public void AsyncSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, AsyncOperation asyncOperation)
    {
        _tracked.Add(asyncOperation);

        foreach (var onSceneLoad in onSceneLoads)
        {
            onSceneLoad.OnSceneLoad(sceneName, sceneBuildIndex, loadSceneMode, localPhysicsMode);
        }

        logger.LogDebug($"async scene load, {asyncOperation.GetHashCode()}");
        _asyncLoads.Add(new(sceneName, sceneBuildIndex, asyncOperation, loadSceneMode, localPhysicsMode));
    }

    public void AllowSceneActivation(bool allow, AsyncOperation asyncOperation)
    {
        logger.LogDebug($"allow scene activation {allow}, {new StackTrace()}");

        if (allow)
        {
            var sceneToLoad = _asyncLoadStalls.Find(x => ReferenceEquals(x.AsyncOperationInstance, asyncOperation));
            if (sceneToLoad == null) return;
            StoreAllowSceneActivation(asyncOperation, true);
            _asyncLoadStalls.Remove(sceneToLoad);
            sceneWrapper.LoadSceneAsync(sceneToLoad.SceneName, sceneToLoad.SceneBuildIndex, sceneToLoad.LoadSceneMode,
                sceneToLoad.LocalPhysicsMode, true);
            logger.LogDebug(
                $"force loading scene, name: {sceneToLoad.SceneName}, build index: {sceneToLoad.SceneBuildIndex}");
            _pendingLoadCallbacks.Add(sceneToLoad.AsyncOperationInstance);
        }
        else
        {
            var asyncSceneLoad = _asyncLoads.Find(x => ReferenceEquals(x.AsyncOperationInstance, asyncOperation));
            if (asyncSceneLoad == null) return;
            StoreAllowSceneActivation(asyncOperation, false);
            _asyncLoads.Remove(asyncSceneLoad);
            _asyncLoadStalls.Add(asyncSceneLoad);
            logger.LogDebug("Added scene to stall list");
        }
    }

    private void StoreAllowSceneActivation(AsyncOperation asyncOperation, bool allow)
    {
        var allowSceneActivationStore =
            _allowSceneActivationValue.FindIndex(tuple => ReferenceEquals(tuple.Item2, asyncOperation));
        if (allowSceneActivationStore >= 0)
        {
            _allowSceneActivationValue[allowSceneActivationStore] = (allow, asyncOperation);
        }
        else
        {
            _allowSceneActivationValue.Add((allow, asyncOperation));
        }
    }

    public bool GetAllowSceneActivation(AsyncOperation asyncOperation, out bool state)
    {
        var stateIndex = _allowSceneActivationValue.FindIndex(x => ReferenceEquals(x.Item2, asyncOperation));
        if (stateIndex >= 0)
        {
            state = _allowSceneActivationValue[stateIndex].Item1;
            return true;
        }

        // not found, is it even tracked?
        state = false;
        return _tracked.Exists(x => ReferenceEquals(x, asyncOperation));
    }

    public bool IsDone(AsyncOperation asyncOperation, out bool isDone)
    {
        if (_isDone.Exists(x => ReferenceEquals(x, asyncOperation)))
        {
            isDone = true;
            return true;
        }

        isDone = false;
        return _tracked.Exists(x => ReferenceEquals(x, asyncOperation));
    }

    public bool Progress(AsyncOperation asyncOperation, out float progress)
    {
        if (_isDone.Exists(x => ReferenceEquals(x, asyncOperation)))
        {
            progress = 1f;
            return true;
        }

        progress = 0.9f;
        if (!_tracked.Exists(x => ReferenceEquals(x, asyncOperation)))
        {
            return false;
        }

        return true;
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

    public bool IsInvokingOnComplete { get; private set; }

    private void InvokeOnComplete(AsyncOperation asyncOperation)
    {
        if (_invokeCompletionEvent == null) return;
        IsInvokingOnComplete = true;
        logger.LogDebug("invoking completion event");
        _invokeCompletionEvent.Invoke(asyncOperation, null);
        IsInvokingOnComplete = false;
    }

    private class AsyncSceneLoadData(
        string sceneName,
        int sceneBuildIndex,
        AsyncOperation asyncOperationInstance,
        LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode)
    {
        public string SceneName { get; } = sceneName;
        public int SceneBuildIndex { get; } = sceneBuildIndex;
        public AsyncOperation AsyncOperationInstance { get; } = asyncOperationInstance;
        public LoadSceneMode LoadSceneMode { get; } = loadSceneMode;
        public LocalPhysicsMode LocalPhysicsMode { get; } = localPhysicsMode;
    }
}
