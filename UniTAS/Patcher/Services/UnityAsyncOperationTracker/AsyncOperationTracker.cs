using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class AsyncOperationTracker(ISceneWrapper sceneWrapper, ILogger logger, IPatchReverseInvoker reverseInvoker)
    : ISceneLoadTracker, IAssetBundleCreateRequestTracker, IAssetBundleRequestTracker,
        IOnLastUpdateUnconditional, IAsyncOperationIsInvokingOnComplete, IOnPreGameRestart, IOnUpdateActual
{
    private readonly HashSet<AsyncOperation> _tracked = new(new HashUtils.ReferenceComparer<AsyncOperation>());

    private readonly Dictionary<AsyncOperation, AsyncSceneLoadData> _asyncLoads = new();
    private readonly List<AsyncOperation> _pendingLoadCallbacks = new();
    private readonly Dictionary<AsyncOperation, AsyncSceneLoadData> _asyncLoadStalls = new();
    private readonly Dictionary<AsyncOperation, AssetBundle> _assetBundleCreateRequests = new();

    private readonly Dictionary<AsyncOperation, AssetBundleRequestData> _assetBundleRequests = new();

    private readonly Dictionary<AsyncOperation, bool> _allowSceneActivationValue =
        new(new HashUtils.ReferenceComparer<AsyncOperation>());

    private readonly HashSet<AsyncOperation> _isDone = new(new HashUtils.ReferenceComparer<AsyncOperation>());

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
        foreach (var pair in _asyncLoads)
        {
            var scene = pair.Value;
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

    public void AsyncSceneUnload(AsyncOperation asyncOperation)
    {
        logger.LogDebug("async scene unload");

        _tracked.Add(asyncOperation);
        _isDone.Add(asyncOperation);
        InvokeOnComplete(asyncOperation);
    }

    public void AsyncSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, AsyncOperation asyncOperation)
    {
        _tracked.Add(asyncOperation);

        logger.LogDebug($"async scene load, {asyncOperation.GetHashCode()}");
        _asyncLoads.Add(asyncOperation,
            new(sceneName, sceneBuildIndex, asyncOperation, loadSceneMode, localPhysicsMode));
    }

    public void NonAsyncSceneLoad()
    {
        foreach (var pair in _asyncLoads.Concat(_asyncLoadStalls))
        {
            var scene = pair.Value;
            logger.LogDebug(
                $"force loading scene, name: {scene.SceneName}, index: {scene.SceneBuildIndex}, manually loading in loop");
            reverseInvoker.Invoke(s => sceneWrapper.LoadSceneAsync(s.SceneName, s.SceneBuildIndex,
                s.LoadSceneMode,
                s.LocalPhysicsMode, true), scene);
            _pendingLoadCallbacks.Add(scene.AsyncOperationInstance);
        }

        _asyncLoads.Clear();
    }

    public void AllowSceneActivation(bool allow, AsyncOperation asyncOperation)
    {
        logger.LogDebug($"allow scene activation {allow}, {new StackTrace()}");

        if (allow)
        {
            if (!_asyncLoadStalls.ContainsKey(asyncOperation)) return;
            StoreAllowSceneActivation(asyncOperation, true);
            var sceneToLoad = _asyncLoadStalls[asyncOperation];
            _asyncLoadStalls.Remove(asyncOperation);
            sceneWrapper.LoadSceneAsync(sceneToLoad.SceneName, sceneToLoad.SceneBuildIndex, sceneToLoad.LoadSceneMode,
                sceneToLoad.LocalPhysicsMode, true);
            logger.LogDebug(
                $"force loading scene, name: {sceneToLoad.SceneName}, build index: {sceneToLoad.SceneBuildIndex}");
            _pendingLoadCallbacks.Add(sceneToLoad.AsyncOperationInstance);
        }
        else
        {
            if (!_asyncLoads.ContainsKey(asyncOperation)) return;
            StoreAllowSceneActivation(asyncOperation, false);
            _asyncLoadStalls.Add(asyncOperation, _asyncLoads[asyncOperation]);
            _asyncLoads.Remove(asyncOperation);
            logger.LogDebug("Added scene to stall list");
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
        logger.LogDebug("invoking completion event");
        _invokeCompletionEvent.Invoke(asyncOperation, null);
        _isInvokingOnComplete = false;
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