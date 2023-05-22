using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Plugin.Models.UnitySafeWrappers.SceneManagement;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniTAS.Plugin.Services.UnityAsyncOperationTracker;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class AsyncOperationTracker : ISceneLoadTracker, IAssetBundleCreateRequestTracker, IAssetBundleRequestTracker,
    IOnLastUpdateUnconditional, IAsyncOperationIsInvokingOnComplete
{
    private readonly List<AsyncSceneLoadData> _asyncLoads = new();
    private readonly List<AsyncSceneLoadData> _asyncLoadStalls = new();
    private readonly Dictionary<AsyncOperation, AssetBundle> _assetBundleCreateRequests = new();
    private readonly Dictionary<AsyncOperation, AssetBundleRequestData> _assetBundleRequests = new();

    private readonly ILogger _logger;

    private class AssetBundleRequestData
    {
        public Object SingleResult { get; }
        public Object[] MultipleResults { get; }

        public AssetBundleRequestData(Object singleResult = null, Object[] multipleResults = null)
        {
            SingleResult = singleResult;
            MultipleResults = multipleResults;
        }
    }

    private readonly ISceneWrapper _sceneWrapper;

    public AsyncOperationTracker(ISceneWrapper sceneWrapper, ILogger logger)
    {
        _sceneWrapper = sceneWrapper;
        _logger = logger;
    }

    public void OnLastUpdateUnconditional()
    {
        foreach (var scene in _asyncLoads)
        {
            _logger.LogDebug(
                $"force loading scene, name: {scene.SceneName}, index: {scene.SceneBuildIndex}, manually loading in loop");
            _sceneWrapper.LoadSceneAsync(scene.SceneName, scene.SceneBuildIndex, scene.LoadSceneMode,
                scene.LocalPhysicsMode, true);

            // event fired
            InvokeOnComplete(scene.AsyncOperationInstance);
        }

        _asyncLoads.Clear();
    }

    public void NewAssetBundleRequest(AsyncOperation asyncOperation, Object assetBundleRequest)
    {
        _assetBundleRequests.Add(asyncOperation, new(assetBundleRequest));
        InvokeOnComplete(asyncOperation);
    }

    public void NewAssetBundleRequestMultiple(AsyncOperation asyncOperation, Object[] assetBundleRequestArray)
    {
        _assetBundleRequests.Add(asyncOperation, new(multipleResults: assetBundleRequestArray));
        InvokeOnComplete(asyncOperation);
    }

    public void NewAssetBundleCreateRequest(AsyncOperation asyncOperation, AssetBundle assetBundle)
    {
        _assetBundleCreateRequests.Add(asyncOperation, assetBundle);
        InvokeOnComplete(asyncOperation);
    }

    public void AsyncSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, AsyncOperation asyncOperation)
    {
        _logger.LogDebug($"async scene load, {asyncOperation.GetHashCode()}");
        _asyncLoads.Add(new(sceneName, sceneBuildIndex, asyncOperation, loadSceneMode, localPhysicsMode));
    }

    public void AllowSceneActivation(bool allow, AsyncOperation asyncOperation)
    {
        _logger.LogDebug($"allow scene activation {allow}, {new StackTrace()}");

        if (allow)
        {
            var sceneToLoad = _asyncLoadStalls.Find(x => ReferenceEquals(x.AsyncOperationInstance, asyncOperation));
            if (sceneToLoad == null) return;
            _asyncLoadStalls.Remove(sceneToLoad);
            _sceneWrapper.LoadSceneAsync(sceneToLoad.SceneName, sceneToLoad.SceneBuildIndex, sceneToLoad.LoadSceneMode,
                sceneToLoad.LocalPhysicsMode, true);
            _logger.LogDebug(
                $"force loading scene, name: {sceneToLoad.SceneName}, build index: {sceneToLoad.SceneBuildIndex}");
            InvokeOnComplete(sceneToLoad.AsyncOperationInstance);
        }
        else
        {
            var asyncSceneLoad = _asyncLoads.Find(x => ReferenceEquals(x.AsyncOperationInstance, asyncOperation));
            if (asyncSceneLoad == null) return;
            _asyncLoads.Remove(asyncSceneLoad);
            _asyncLoadStalls.Add(asyncSceneLoad);
            _logger.LogDebug("Added scene to stall list");
        }
    }

    public bool IsStalling(AsyncOperation asyncOperation)
    {
        return _asyncLoadStalls.Exists(x => ReferenceEquals(x.AsyncOperationInstance, asyncOperation));
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
        _logger.LogDebug("invoking completion event");
        _invokeCompletionEvent.Invoke(asyncOperation, null);
        IsInvokingOnComplete = false;
    }

    private class AsyncSceneLoadData
    {
        public string SceneName { get; }
        public int SceneBuildIndex { get; }
        public AsyncOperation AsyncOperationInstance { get; }
        public LoadSceneMode LoadSceneMode { get; }
        public LocalPhysicsMode LocalPhysicsMode { get; }

        public AsyncSceneLoadData(string sceneName, int sceneBuildIndex, AsyncOperation asyncOperationInstance,
            LoadSceneMode loadSceneMode, LocalPhysicsMode localPhysicsMode)
        {
            SceneName = sceneName;
            SceneBuildIndex = sceneBuildIndex;
            AsyncOperationInstance = asyncOperationInstance;
            LoadSceneMode = loadSceneMode;
            LocalPhysicsMode = localPhysicsMode;
        }
    }
}