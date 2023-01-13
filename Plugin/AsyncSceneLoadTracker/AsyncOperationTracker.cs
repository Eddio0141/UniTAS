using System.Collections.Generic;
using System.Diagnostics;
using UniTASPlugin.UnitySafeWrappers.Interfaces;

namespace UniTASPlugin.AsyncSceneLoadTracker;

public class AsyncOperationTracker : ISceneLoadTracker, IAssetBundleCreateRequestTracker, IAssetBundleRequestTracker
{
    private readonly List<AsyncSceneLoadData> _asyncLoads = new();
    private readonly List<AsyncSceneLoadData> _asyncLoadStalls = new();
    private readonly Dictionary<int, object> _assetBundleCreateRequests = new();
    private readonly Dictionary<int, AssetBundleRequestData> _assetBundleRequests = new();

    // TODO add late update to load _asyncLoads scenes
    /*
     * 
            foreach (var scene in asyncSceneLoads)
            {
                Plugin.Log.LogDebug($"force loading scene, name: {scene.sceneName} {scene.sceneBuildIndex}");
                SceneHelper.LoadSceneAsyncNameIndexInternal(scene.sceneName, scene.sceneBuildIndex, scene.parameters,
                    scene.isAdditive, true);
            }
    
            asyncSceneLoads.Clear();
     */

    private class AssetBundleRequestData
    {
        public object SingleResult { get; }
        public object MultipleResults { get; }

        public AssetBundleRequestData(object singleResult = null, object multipleResults = null)
        {
            SingleResult = singleResult;
            MultipleResults = multipleResults;
        }
    }

    private readonly ISceneWrapper _sceneWrapper;

    public AsyncOperationTracker(ISceneWrapper sceneWrapper)
    {
        _sceneWrapper = sceneWrapper;
    }

    public void NewAssetBundleRequest(object asyncOperation, object assetBundleRequest)
    {
        _assetBundleRequests.Add(asyncOperation.GetHashCode(), new(assetBundleRequest));
    }

    public void NewAssetBundleRequestMultiple(object asyncOperation, object assetBundleRequestArray)
    {
        _assetBundleRequests.Add(asyncOperation.GetHashCode(), new(multipleResults: assetBundleRequestArray));
    }

    public void AsyncSceneLoad(string sceneName, int sceneBuildIndex, object parameters, bool? isAdditive,
        object asyncOperation)
    {
        _asyncLoads.Add(new(sceneName, sceneBuildIndex, parameters, isAdditive,
            asyncOperation.GetHashCode()));
    }

    public void AllowSceneActivation(bool allow, object asyncOperation)
    {
        var hash = asyncOperation.GetHashCode();
        Trace.Write($"allow scene activation {allow} for ID {hash}");

        if (allow)
        {
            var sceneToLoad = _asyncLoadStalls.Find(x => x.AsyncOperationInstanceHash == hash);
            if (sceneToLoad == null) return;
            _asyncLoadStalls.Remove(sceneToLoad);
            _sceneWrapper.LoadSceneAsync(sceneToLoad.SceneName, sceneToLoad.SceneBuildIndex, sceneToLoad.Parameters,
                sceneToLoad.IsAdditive, true);
            Trace.Write(
                $"force loading scene, name: {sceneToLoad.SceneName}, build index: {sceneToLoad.SceneBuildIndex}");
        }
        else
        {
            var asyncSceneLoad = _asyncLoads.Find(x => x.AsyncOperationInstanceHash == hash);
            if (asyncSceneLoad == null) return;
            _asyncLoads.Remove(asyncSceneLoad);
            _asyncLoadStalls.Add(asyncSceneLoad);
            Trace.Write("Added scene to stall list");
        }
    }

    public void AsyncOperationDestruction(object asyncOperation)
    {
        _asyncLoadStalls.RemoveAll(x => x.AsyncOperationInstanceHash == asyncOperation.GetHashCode());
        _asyncLoads.RemoveAll(x => x.AsyncOperationInstanceHash == asyncOperation.GetHashCode());
        _assetBundleCreateRequests.Remove(asyncOperation.GetHashCode());
    }

    public bool IsStalling(object asyncOperation)
    {
        return _asyncLoadStalls.Exists(x => x.AsyncOperationInstanceHash == asyncOperation.GetHashCode());
    }

    public void NewAssetBundleCreateRequest(object asyncOperation, object assetBundle)
    {
        _assetBundleCreateRequests.Add(asyncOperation.GetHashCode(), assetBundle);
    }

    public object GetAssetBundleCreateRequest(object asyncOperation)
    {
        return _assetBundleCreateRequests.TryGetValue(asyncOperation.GetHashCode(), out var assetBundle)
            ? assetBundle
            : null;
    }

    public object GetAssetBundleRequest(object asyncOperation)
    {
        return _assetBundleRequests.TryGetValue(asyncOperation.GetHashCode(), out var assetBundleRequest)
            ? assetBundleRequest.SingleResult
            : null;
    }

    public object GetAssetBundleRequestMultiple(object asyncOperation)
    {
        return _assetBundleRequests.TryGetValue(asyncOperation.GetHashCode(), out var assetBundleRequest)
            ? assetBundleRequest.MultipleResults
            : null;
    }

    private class AsyncSceneLoadData
    {
        public string SceneName { get; }
        public int SceneBuildIndex { get; }
        public object Parameters { get; }
        public bool? IsAdditive { get; }
        public int AsyncOperationInstanceHash { get; }

        public AsyncSceneLoadData(string sceneName, int sceneBuildIndex, object parameters, bool? isAdditive,
            int asyncOperationInstanceHash)
        {
            SceneName = sceneName;
            SceneBuildIndex = sceneBuildIndex;
            Parameters = parameters;
            IsAdditive = isAdditive;
            AsyncOperationInstanceHash = asyncOperationInstanceHash;
        }
    }
}