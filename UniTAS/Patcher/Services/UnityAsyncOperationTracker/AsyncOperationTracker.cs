using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
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
using UniTAS.Patcher.Services.UnitySafeWrappers;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class AsyncOperationTracker : ISceneLoadTracker, IAssetBundleCreateRequestTracker, IAssetBundleRequestTracker,
    IOnLastUpdateUnconditional, IAsyncOperationIsInvokingOnComplete, IOnPreGameRestart, IOnUpdateActual,
    IOnStartActual, IOnFixedUpdateActual, IAssetBundleTracker, ISceneOverride
{
    private bool _isInvokingOnComplete;
    private readonly ISceneManagerWrapper _sceneManagerWrapper;
    private readonly ILogger _logger;
    private readonly IGameBuildScenesInfo _gameBuildScenesInfo;
    private readonly IUnityInstanceWrapFactory _wrapFactory;

    public AsyncOperationTracker(ISceneManagerWrapper sceneManagerWrapper, ILogger logger,
        IGameBuildScenesInfo gameBuildScenesInfo, IUnityInstanceWrapFactory wrapFactory)
    {
        _sceneManagerWrapper = sceneManagerWrapper;
        _logger = logger;
        _gameBuildScenesInfo = gameBuildScenesInfo;
        _wrapFactory = wrapFactory;
        var getAllScenePaths = AccessTools.Method(typeof(AssetBundle), "GetAllScenePaths");
        _getAllScenePaths = getAllScenePaths?.MethodDelegate<Func<AssetBundle, string[]>>();
        _loaded = [GetSceneInfo(0)];
    }

    private readonly HashSet<AsyncOperation> _tracked = [];

    // any scene related things are stored as a list
    // it is a list of scenes in load order
    private readonly List<Either<AsyncSceneLoadData, AsyncSceneUnloadData>> _asyncLoads = [];

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

    private int _sceneStructHandleId = int.MaxValue / 2;

    // loaded in this game session
    private readonly HashSet<SceneInfo> _loaded;

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
        DummyScenes.Clear();
        LoadingScenes.Clear();
        LoadingSceneCount = 0;
        _sceneLoadSync = false;
        _sceneStructHandleId = int.MaxValue / 2;
        _loaded.Clear();
        _loaded.Add(GetSceneInfo(0));
        _firstMatchUnloadPaths.Clear();
    }

    public void OnLastUpdateUnconditional()
    {
        _firstMatchUnloadPaths.Clear();

        if (_sceneLoadSync)
        {
            foreach (var loadData in _asyncLoads)
            {
                ProcessLoadFromData(loadData);
            }

            _asyncLoads.Clear();
            return;
        }

        if (_asyncLoads.Count == 0) return;

        var removes = new List<int>();
        var foundLoad = false;
        for (var i = 0; i < _asyncLoads.Count; i++)
        {
            var load = _asyncLoads[i];

            if (!foundLoad)
            {
                if (load.IsLeft)
                {
                    foundLoad = true;
                    if (load.Left.DelayFrame) break;
                }

                ProcessLoadFromData(load);
                removes.Add(i);
                continue;
            }

            if (load.IsLeft) break;
            // add the rest of the unloads
            ProcessLoadFromData(load);
            removes.Add(i);
        }

        foreach (var i in ((IEnumerable<int>)removes).Reverse())
        {
            _asyncLoads.RemoveAt(i);
        }
    }

    private void ProcessLoadFromData(Either<AsyncSceneLoadData, AsyncSceneUnloadData> data)
    {
        if (data.IsLeft)
        {
            var loadData = data.Left;
            _logger.LogDebug(
                $"force loading scene via OnLastUpdate, name: {loadData.SceneName}, index: {loadData.SceneBuildIndex}");
            _sceneManagerWrapper.TrackSceneCountDummy = false;
            _sceneManagerWrapper.LoadSceneAsync(loadData.SceneName, loadData.SceneBuildIndex, loadData.LoadSceneMode,
                loadData.LocalPhysicsMode, true);
            _sceneManagerWrapper.TrackSceneCountDummy = true;

            // call after scene is loaded
            ReplaceDummyScene(loadData.AsyncOperation, loadData.SceneInfo.Path);

            _pendingLoadCallbacks.Add(loadData);
            _loaded.Add(loadData.SceneInfo);
            var op = loadData.AsyncOperation;
            if (op == null)
                return;

            _allowSceneActivationNotAllowed.Remove(op);
            return;
        }

        var unloadData = data.Right;
        _logger.LogWarning(
            "async unload call: THIS OPERATION MIGHT BREAK THE GAME, scene unloading patch is using an unstable unity function, and it may fail");
        _sceneManagerWrapper.TrackSceneCountDummy = false;
        _sceneManagerWrapper.UnloadSceneAsync(unloadData.SceneWrapper.Name, -1, unloadData.Options, true,
            out var success);
        _sceneManagerWrapper.TrackSceneCountDummy = true;
        if (!success)
        {
            _logger.LogError("async unload failed, prepare for game to maybe go nuts");
            return;
        }

        _pendingLoadCallbacks.Add(unloadData);
    }

    public void FixedUpdateActual() => CallPendingCallbacks();

    public void UpdateActual()
    {
        _sceneLoadSync = false;

        // doesn't matter, as long as next frame happens, before the game update adds more scenes, delay is gone
        foreach (var op in _asyncLoads)
        {
            if (op.IsLeft)
                op.Left.DelayFrame = false;
        }

        foreach (var load in _asyncLoadStalls)
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
                    _sceneManagerWrapper.LoadedSceneCountDummy++;
                else
                    _sceneManagerWrapper.LoadedSceneCountDummy = 1;

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

    // silly but simple and efficient enough check
    private readonly HashSet<string> _firstMatchUnloadPaths = [];

    public void AsyncSceneUnload(ref AsyncOperation asyncOperation, Either<string, int> scene, object options)
    {
        _logger.LogDebug("async scene unload, " + (scene.IsLeft ? $"name: {scene.Left}" : $"index: {scene.Right}"));

        var sceneInfo = GetSceneInfo(scene);
        if (sceneInfo == null || !_loaded.Contains(sceneInfo))
        {
            throw new ArgumentException("Scene to unload is invalid");
        }

        if (_sceneManagerWrapper.LoadedSceneCountDummy + LoadingSceneCount == 1)
        {
            _logger.LogDebug("there is only 1 scene loaded, cannot unload, skipping this operation");
            asyncOperation = null;
            return;
        }

        if (_firstMatchUnloadPaths.Contains(sceneInfo.Path))
        {
            _logger.LogDebug("scene is already to be unloaded, skipping this operation");
            asyncOperation = null;
            return;
        }

        _firstMatchUnloadPaths.Add(sceneInfo.Path);

        SceneWrapper sceneFound = null;
        var sceneCount = _sceneManagerWrapper.SceneCount;
        for (var i = 0; i < sceneCount; i++)
        {
            // shouldn't be null, if unity version is so old Scene struct doesn't exist, async unload also shouldn't exist neither
            var sceneWrapper = _sceneManagerWrapper.GetSceneAt(i);
            if (sceneWrapper.Path != sceneInfo.Path) continue;
            sceneFound = sceneWrapper;
            break;
        }

        _tracked.Add(asyncOperation);
        _asyncLoads.Add(new AsyncSceneUnloadData(asyncOperation, options, sceneFound,
            ResolveDummyHandle(sceneFound!.Handle)));
        _sceneManagerWrapper.LoadedSceneCountDummy--;
        LoadingSceneCount++;
    }

    public void AsyncSceneUnload(ref AsyncOperation asyncOperation, object scene, object options)
    {
        var sceneWrap = _wrapFactory.Create<SceneWrapper>(scene);
        _logger.LogDebug($"async scene unload, {sceneWrap.Path}");
        var a = DummyScenes.FindIndex(x => x.dummyScene.TrackingHandle == sceneWrap.Handle);
        if (a >= 0)
        {
            StaticLogger.LogDebug($"thingy: {DummyScenes[a].actualScene.Path}");
        }

        if (_sceneManagerWrapper.LoadedSceneCountDummy + LoadingSceneCount == 1)
        {
            _logger.LogDebug("there is only 1 scene loaded, cannot unload, skipping this operation");
            asyncOperation = null;
            return;
        }

        var handle = ResolveDummyHandle(sceneWrap.Handle);

        if (_asyncLoads.Any(x => x.IsRight && x.Right.SceneWrapper.Handle == handle))
        {
            _logger.LogDebug("scene is already to be unloaded, skipping this operation");
            asyncOperation = null;
            return;
        }

        _firstMatchUnloadPaths.Add(sceneWrap.Path);

        _tracked.Add(asyncOperation);
        _asyncLoads.Add(new AsyncSceneUnloadData(asyncOperation, options, sceneWrap, handle));
        _sceneManagerWrapper.LoadedSceneCountDummy--;
        LoadingSceneCount++;
    }

    private int ResolveDummyHandle(int handle)
    {
        var dummySceneIndex = DummyScenes.FindIndex(x => x.dummyScene.TrackingHandle == handle);
        return dummySceneIndex >= 0 ? DummyScenes[dummySceneIndex].actualScene?.Handle ?? handle : handle;
    }

    public void AsyncSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, ref AsyncOperation asyncOperation)
    {
        Either<string, int> scene = sceneBuildIndex >= 0 ? sceneBuildIndex : sceneName;
        if (InvalidSceneLoadAndLog(scene, out var sceneInfo))
        {
            asyncOperation = null;
            return;
        }

        LoadingSceneCount++;
        CreateDummySceneStruct(scene, asyncOperation);

        _tracked.Add(asyncOperation);

        _logger.LogDebug(
            $"async scene load, {sceneName}, index: {sceneBuildIndex}, loadSceneMode: {loadSceneMode}, localPhysicsMode: {localPhysicsMode}");
        var loadData =
            new AsyncSceneLoadData(sceneName, sceneBuildIndex, loadSceneMode, localPhysicsMode, asyncOperation,
                sceneInfo);
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
        Either<string, int> scene = sceneBuildIndex >= 0 ? sceneBuildIndex : sceneName;
        if (InvalidSceneLoadAndLog(scene, out var sceneInfo)) return;
        _logger.LogDebug(
            $"scene load, {sceneName}, index: {sceneBuildIndex}, loadSceneMode: {loadSceneMode}, localPhysicsMode: {localPhysicsMode}");

        if (!_sceneLoadSync)
        {
            _sceneLoadSync = true;

            // first, all stalls are moved to load
            foreach (var load in _asyncLoadStalls)
            {
                _asyncLoads.Add(load);
            }

            _asyncLoadStalls.Clear();

            foreach (var sceneData in _asyncLoads)
            {
                // delays are all gone now
                if (!sceneData.IsLeft) continue;

                var left = sceneData.Left;
                left.DelayFrame = false;
                _logger.LogDebug(
                    $"force loading scene via non-async scene load, name: {left.SceneName}, index: {left.SceneBuildIndex}");
                _allowSceneActivationNotAllowed.Add(left.AsyncOperation);
            }
        }

        // next, queue the non async scene load
        LoadingSceneCount++;
        CreateDummySceneStruct(scene, null);

        _asyncLoads.Add(new AsyncSceneLoadData(sceneName, sceneBuildIndex, loadSceneMode, localPhysicsMode, null,
            sceneInfo));
    }

    private bool InvalidSceneLoadAndLog(Either<string, int> scene, out SceneInfo sceneInfo)
    {
        sceneInfo = GetSceneInfo(scene);
        if (sceneInfo != null) return false;

        var errorBuilder = new StringBuilder();

        errorBuilder.AppendLine(
            scene.IsLeft
                ? $"Scene '{scene.Left}' couldn't be loaded because it has not been added to the build settings or the AssetBundle has not been loaded."
                : $"Scene with build index: {scene.Right} couldn't be loaded because it has not been added to the build settings.");

        errorBuilder.Append("To add a scene to the build settings use the menu File->Build Settings...");
        Debug.LogError(errorBuilder);
        return true;
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
            var loadIndex = _asyncLoads.FindIndex(d => d.IsLeft && d.Left.AsyncOperation == asyncOperation);
            if (loadIndex < 0) return;
            StoreAllowSceneActivation(asyncOperation, false);
            if (_allowSceneActivationNotAllowed.Contains(asyncOperation)) return;
            _asyncLoadStalls.Add(_asyncLoads[loadIndex].Left);
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

        if (_asyncLoads.Concat(_pendingLoadCallbacks)
            .Any(load => load.IsRight && load.Right.AsyncOperation == asyncOperation))
        {
            progress = 0f;
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
        try
        {
            _invokeCompletionEvent.Invoke(asyncOperation, null);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        _isInvokingOnComplete = false;
    }

    // note: this property exists here, because this class handles async scene loading, not scene wrapper
    public int LoadingSceneCount { get; private set; }

    public List<(DummyScene dummyScene, SceneWrapper actualScene)> DummyScenes { get; } = new();
    public List<DummyScene> LoadingScenes { get; } = new();

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

    private void CreateDummySceneStruct(Either<string, int> load, AsyncOperation op)
    {
        if (_sceneStruct == null) return;
        var sceneInfo = GetSceneInfo(load);
        if (sceneInfo == null) return;

        var instance = _wrapFactory.Create<SceneWrapper>(null);
        instance.Handle = _sceneStructHandleId;
        var dummyScene = new DummyScene(instance.Instance, _sceneStructHandleId, sceneInfo, op);
        LoadingScenes.Add(dummyScene);
        DummyScenes.Add((dummyScene, null));
        _sceneStructHandleId++;
    }

    private void ReplaceDummyScene(AsyncOperation op, string path)
    {
        var sceneCount = _sceneManagerWrapper.SceneCount;
        if (sceneCount < 0)
        {
            _logger.LogWarning("Scene count is invalid");
            return;
        }

        if (LoadingScenes.Count == 0)
        {
            _logger.LogWarning("Usually there are loading scenes, but there isn't");
            return;
        }

        var actualScene = _sceneManagerWrapper.GetSceneAt(sceneCount - 1);
        var loadingInfoIndex = op == null
            ? LoadingScenes.FindIndex(x => x.Op == null && x.LoadingScene.Path == path)
            : LoadingScenes.FindIndex(x => ReferenceEquals(x.Op, op));
        var dummyHandle = LoadingScenes[loadingInfoIndex].TrackingHandle;
        LoadingScenes.RemoveAt(loadingInfoIndex);
        var dummySceneIndex = DummyScenes.FindIndex(x => x.dummyScene.TrackingHandle == dummyHandle);
        DummyScenes[dummySceneIndex] = (DummyScenes[dummySceneIndex].dummyScene, actualScene);
    }

    private SceneInfo GetSceneInfo(Either<string, int> scene)
    {
        if (scene.IsRight)
        {
            var buildIndex = scene.Right;
            if (buildIndex >= _gameBuildScenesInfo.IndexToPath.Count) return null;
            var path = _gameBuildScenesInfo.IndexToPath[buildIndex];
            var name = _gameBuildScenesInfo.PathToName[path];
            return new(path, name, buildIndex);
        }

        var name2 = scene.Left;

        // find info about scene
        if (_gameBuildScenesInfo.NameToPath.TryGetValue(name2, out var path2))
        {
            // sceneName is name
            return new(path2, name2, _gameBuildScenesInfo.PathToIndex[path2]);
        }

        if (_gameBuildScenesInfo.PathToIndex.TryGetValue(name2, out var index2))
        {
            // sceneName is path
            return new(name2, _gameBuildScenesInfo.PathToName[name2], index2);
        }
        // TODO: in AssetBundle scenes, can i load from just name, or do i need path
        // bundleScenes contains paths, not names

        if (_bundleScenes.Values.Any(scenes => scenes.Any(s => s == name2)))
        {
            // TODO: fix below after resolving above
            return new(name2, name2, -1);
        }

        if (_getAllScenePaths == null)
        {
            throw new InvalidOperationException(
                "Tried to search for scene name but scene wasn't found, this may be wrong as " +
                "AssetBundle.GetAllScenePaths doesn't exist in this unity version");
        }

        return null;
    }

    public bool IsLoaded(int handle, out bool loaded)
    {
        if (_asyncLoads.Any(x => x.IsRight && x.Right.RealHandle == ResolveDummyHandle(handle)))
        {
            loaded = false;
            return true;
        }

        loaded = false;
        return false;
    }

    private class AsyncSceneLoadData(
        string sceneName,
        int sceneBuildIndex,
        LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode,
        AsyncOperation asyncOperation,
        SceneInfo sceneInfo)
    {
        public string SceneName { get; } = sceneName;
        public int SceneBuildIndex { get; } = sceneBuildIndex;
        public LoadSceneMode LoadSceneMode { get; } = loadSceneMode;
        public LocalPhysicsMode LocalPhysicsMode { get; } = localPhysicsMode;
        public AsyncOperation AsyncOperation { get; } = asyncOperation;
        public bool DelayFrame = true;
        public SceneInfo SceneInfo { get; } = sceneInfo;
    }

    private class AsyncSceneUnloadData(
        AsyncOperation asyncOperation,
        object options,
        SceneWrapper sceneWrapper,
        int realHandle)
    {
        public AsyncOperation AsyncOperation { get; } = asyncOperation;
        public object Options { get; } = options;
        public SceneWrapper SceneWrapper { get; } = sceneWrapper;
        public int RealHandle { get; } = realHandle;
    }

    public class SceneInfo(string path, string name, int buildIndex)
    {
        private bool Equals(SceneInfo other)
        {
            return string.Equals(Path, other.Path, StringComparison.InvariantCulture) &&
                   string.Equals(Name, other.Name, StringComparison.InvariantCulture) && BuildIndex == other.BuildIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SceneInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Path != null ? StringComparer.InvariantCulture.GetHashCode(Path) : 0;
                hashCode = (hashCode * 397) ^ (Name != null ? StringComparer.InvariantCulture.GetHashCode(Name) : 0);
                hashCode = (hashCode * 397) ^ BuildIndex;
                return hashCode;
            }
        }

        public static bool operator ==(SceneInfo left, SceneInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SceneInfo left, SceneInfo right)
        {
            return !Equals(left, right);
        }

        public string Path { get; } = path;
        public string Name { get; } = name;
        public int BuildIndex { get; } = buildIndex;
    }

    public readonly struct DummyScene(
        object dummySceneStruct,
        int trackingHandle,
        SceneInfo loadingScene,
        AsyncOperation op)
    {
        public readonly object DummySceneStruct = dummySceneStruct;
        public readonly int TrackingHandle = trackingHandle;
        public readonly SceneInfo LoadingScene = loadingScene;
        public readonly AsyncOperation Op = op;
    }
}