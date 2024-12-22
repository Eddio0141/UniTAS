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
    IOnLastUpdateActual, IAsyncOperationIsInvokingOnComplete, IOnPreGameRestart, IOnUpdateActual,
    IOnStartActual, IOnFixedUpdateActual, IAssetBundleTracker, ISceneOverride, IAsyncOperationOverride
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

    private readonly Dictionary<AsyncOperation, AsyncOperationData> _tracked = [];

    // all async operations queued up in order
    private readonly List<IAsyncOperation> _ops = [];
    private readonly List<IAsyncOperation> _pendingLoadCallbacks = [];

    private readonly Dictionary<AsyncOperation, AssetBundle> _assetBundleCreateRequests = new();
    private readonly Dictionary<AsyncOperation, AssetBundleRequestData> _assetBundleRequests = new();
    private readonly Dictionary<AssetBundle, string[]> _bundleScenes = new();

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
        _ops.Clear();
        _pendingLoadCallbacks.Clear();
        _assetBundleCreateRequests.Clear();
        _assetBundleRequests.Clear();
        _tracked.Clear();
        _bundleScenes.Clear();
        DummyScenes.Clear();
        LoadingScenes.Clear();
        LoadingSceneCount = 0;
        _sceneLoadSync = false;
        _sceneStructHandleId = int.MaxValue / 2;
        _loaded.Clear();
        _loaded.Add(GetSceneInfo(0));
        _firstMatchUnloadPaths.Clear();
        _subSceneTracker.Clear();
    }

    public void OnLastUpdateActual()
    {
        _firstMatchUnloadPaths.Clear();

        if (_sceneLoadSync)
        {
            foreach (var loadData in _ops)
            {
                loadData.Load();
                _pendingLoadCallbacks.Add(loadData);
            }

            _ops.Clear();
            return;
        }

        if (_ops.Count == 0) return;

        var removes = new List<int>();
        var foundLoad = false;
        for (var i = 0; i < _ops.Count; i++)
        {
            var load = _ops[i];

            if (!foundLoad)
            {
                if (load is AsyncSceneLoadData data)
                {
                    foundLoad = true;
                    if (data.DelayFrame) break;

                    var op = load.Op;
                    if (op != null)
                    {
                        var trackState = _tracked[load.Op];
                        if (trackState is { NotAllowedToStall: false, AllowSceneActivation: false }) break;
                    }
                }

                load.Load();
                _pendingLoadCallbacks.Add(load);
                removes.Add(i);
                continue;
            }

            if (load is AsyncSceneLoadData) break;
            // add the rest of the operations
            load.Load();
            removes.Add(i);
        }

        foreach (var i in ((IEnumerable<int>)removes).Reverse())
        {
            _ops.RemoveAt(i);
        }
    }

    public void FixedUpdateActual() => CallPendingCallbacks();

    public void UpdateActual()
    {
        _sceneLoadSync = false;

        // doesn't matter, as long as next frame happens, before the game update adds more scenes, delay is gone
        foreach (var op in _ops)
        {
            if (op is AsyncSceneLoadData data)
                data.DelayFrame = false;
        }

        CallPendingCallbacks();
    }

    public void StartActual() => CallPendingCallbacks();

    private void CallPendingCallbacks()
    {
        if (_pendingLoadCallbacks.Count == 0) return;

        // to allow the scene to be findable, invoke when scene loads on update
        var callbacks = new List<IAsyncOperation>(_pendingLoadCallbacks.Count);
        foreach (var data in _pendingLoadCallbacks)
        {
            callbacks.Add(data);
            var op = data.Op;
            if (op == null) continue;
            var state = _tracked[op];
            state.IsDone = true;
            _tracked[op] = state;
        }

        // prevent allowSceneActivation to be False on event callback
        _pendingLoadCallbacks.Clear();

        foreach (var data in callbacks)
        {
            data.Callback();
            if (data.Op == null) continue;
            InvokeOnComplete(data.Op);
        }
    }

    public void NewAssetBundleRequest(AsyncOperation asyncOperation, Object assetBundleRequest)
    {
        _assetBundleRequests.Add(asyncOperation, new AssetBundleRequestData(assetBundleRequest));
        _tracked.Add(asyncOperation, new AsyncOperationData { IsDone = true });
        InvokeOnComplete(asyncOperation);
    }

    public void NewAssetBundleRequestMultiple(AsyncOperation asyncOperation, Object[] assetBundleRequestArray)
    {
        _assetBundleRequests.Add(asyncOperation, new AssetBundleRequestData(multipleResults: assetBundleRequestArray));
        _tracked.Add(asyncOperation, new AsyncOperationData { IsDone = true });
        InvokeOnComplete(asyncOperation);
    }

    public void NewAssetBundleCreateRequest(AsyncOperation asyncOperation, AssetBundle assetBundle)
    {
        _assetBundleCreateRequests.Add(asyncOperation, assetBundle);
        if (_getAllScenePaths != null)
            _bundleScenes[assetBundle] = _getAllScenePaths(assetBundle);
        _tracked.Add(asyncOperation, new AsyncOperationData { IsDone = true });
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

        _tracked.Add(asyncOperation, new AsyncOperationData());
        _ops.Add(new AsyncSceneUnloadData(asyncOperation, options, sceneFound, ResolveDummyHandle(sceneFound!.Handle),
            this));
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

        if (_ops.Any(x => x is AsyncSceneUnloadData d && d.SceneWrapper.Handle == handle))
        {
            _logger.LogDebug("scene is already to be unloaded, skipping this operation");
            asyncOperation = null;
            return;
        }

        _firstMatchUnloadPaths.Add(sceneWrap.Path);

        _tracked.Add(asyncOperation, new AsyncOperationData());
        _ops.Add(new AsyncSceneUnloadData(asyncOperation, options, sceneWrap, handle, this));
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

        _tracked.Add(asyncOperation, new AsyncOperationData());

        _logger.LogDebug(
            $"async scene load, {sceneName}, index: {sceneBuildIndex}, loadSceneMode: {loadSceneMode}, localPhysicsMode: {localPhysicsMode}");
        var loadData =
            new AsyncSceneLoadData(sceneName, sceneBuildIndex, loadSceneMode, localPhysicsMode, asyncOperation,
                sceneInfo, this);
        _ops.Add(loadData);

        if (!_sceneLoadSync) return;
        _logger.LogDebug("load scene sync is happening for next frame");

        // sync load is going to happen, make this load too
        loadData.DelayFrame = false;
        var data = _tracked[asyncOperation];
        data.NotAllowedToStall = true;
        _tracked[asyncOperation] = data;
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
            foreach (var pair in _tracked.ToList())
            {
                var op = pair.Key;
                var data = pair.Value;
                data.AllowSceneActivation = true;
                data.NotAllowedToStall = true;
                _tracked[op] = data;
            }

            foreach (var sceneData in _ops)
            {
                // delays are all gone now
                if (sceneData is not AsyncSceneLoadData data) continue;

                data.DelayFrame = false;
                _logger.LogDebug("force loading scene via non-async scene load");
            }
        }

        // next, queue the non async scene load
        LoadingSceneCount++;
        CreateDummySceneStruct(scene, null);

        _ops.Add(new AsyncSceneLoadData(sceneName, sceneBuildIndex, loadSceneMode, localPhysicsMode, null,
            sceneInfo, this));
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

        var state = _tracked[asyncOperation];
        state.AllowSceneActivation = allow;
        _tracked[asyncOperation] = state;
        if (state.NotAllowedToStall) return;

        _logger.LogDebug(allow ? "restored scene activation" : "scene load is stalled");
    }

    public bool GetAllowSceneActivation(AsyncOperation asyncOperation, out bool state)
    {
        if (_tracked.TryGetValue(asyncOperation, out var data))
        {
            state = data.AllowSceneActivation;
            return true;
        }

        state = false;
        return false;
    }

    public bool IsDone(AsyncOperation asyncOperation, out bool isDone)
    {
        if (_tracked.TryGetValue(asyncOperation, out var data))
        {
            isDone = data.IsDone;
            return true;
        }

        isDone = false;
        return false;
    }

    public bool Progress(AsyncOperation asyncOperation, out float progress)
    {
        if (_ops.Any(load => load is AsyncSceneUnloadData && ReferenceEquals(load.Op, asyncOperation)))
        {
            progress = 0f;
            return true;
        }

        if (_tracked.TryGetValue(asyncOperation, out var data))
        {
            progress = data.IsDone ? 1f : 0.9f;
            return true;
        }

        progress = 0f;
        return false;
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
        if (_tracked.ContainsKey(asyncOperation))
        {
            wasInvoked = _isInvokingOnComplete;
            return true;
        }

        wasInvoked = false;
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

    // tracks sub scene for dummy scenes during load
    private readonly Dictionary<int, bool> _subSceneTracker = new();

    public void Unload(AssetBundle assetBundle)
    {
        _bundleScenes.Remove(assetBundle);
    }

    public void UnloadBundleAsync(AsyncOperation op)
    {
        _tracked.Add(op, new AsyncOperationData { IsDone = true });
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

        // sub scene value copied
        if (_subSceneTracker.TryGetValue(dummyHandle, out var sub))
        {
            actualScene.IsSubScene = sub;
        }
    }

    private SceneInfo GetSceneInfo(Either<string, int> scene)
    {
        if (scene.IsRight)
        {
            var buildIndex = scene.Right;
            if (buildIndex >= _gameBuildScenesInfo.IndexToPath.Count) return null;
            var path = _gameBuildScenesInfo.IndexToPath[buildIndex];
            var name = _gameBuildScenesInfo.PathToName[path];
            return new SceneInfo(path, name, buildIndex);
        }

        var name2 = scene.Left;

        // find info about scene
        if (_gameBuildScenesInfo.NameToPath.TryGetValue(name2, out var path2))
        {
            // sceneName is name
            return new SceneInfo(path2, name2, _gameBuildScenesInfo.PathToIndex[path2]);
        }

        if (_gameBuildScenesInfo.PathToIndex.TryGetValue(name2, out var index2))
        {
            // sceneName is path
            return new SceneInfo(name2, _gameBuildScenesInfo.PathToName[name2], index2);
        }
        // TODO: in AssetBundle scenes, can i load from just name, or do i need path
        // bundleScenes contains paths, not names

        if (_bundleScenes.Values.Any(scenes => scenes.Any(s => s == name2)))
        {
            // TODO: fix below after resolving above
            return new SceneInfo(name2, name2, -1);
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
        if (_ops.Any(x => x is AsyncSceneUnloadData data && data.RealHandle == ResolveDummyHandle(handle)))
        {
            loaded = false;
            return true;
        }

        loaded = false;
        return false;
    }

    public bool IsSubScene(int handle, out bool subScene)
    {
        if (_subSceneTracker.TryGetValue(handle, out var sub))
        {
            subScene = sub;
            return true;
        }

        subScene = false;
        return false;
    }

    public bool SetSubScene(int handle, bool subScene)
    {
        if (DummyScenes.FindIndex(x => x.dummyScene.TrackingHandle == handle) < 0) return false;
        _subSceneTracker[handle] = subScene;
        return true;
    }

    public bool GetPriority(AsyncOperation op, out int priority)
    {
        if (_tracked.TryGetValue(op, out var data))
        {
            priority = data.Priority;
            return true;
        }

        priority = 0;
        return false;
    }

    public bool SetPriority(AsyncOperation op, int priority)
    {
        if (!_tracked.TryGetValue(op, out var data)) return false;

        data.Priority = priority;
        _tracked[op] = data;
        return true;
    }

    private struct AsyncOperationData()
    {
        public bool IsDone = false;
        public bool AllowSceneActivation = true;
        public bool NotAllowedToStall = false;
        public int Priority = 0;
    }

    private class AsyncSceneLoadData(
        string sceneName,
        int sceneBuildIndex,
        LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode,
        AsyncOperation asyncOperation,
        SceneInfo sceneInfo,
        AsyncOperationTracker tracker) : IAsyncOperation
    {
        public AsyncOperation Op { get; } = asyncOperation;
        public bool DelayFrame = true;

        public void Load()
        {
            tracker._logger.LogDebug(
                $"force loading scene via OnLastUpdate, name: {sceneName}, index: {sceneBuildIndex}");
            tracker._sceneManagerWrapper.TrackSceneCountDummy = false;
            tracker._sceneManagerWrapper.LoadSceneAsync(sceneName, sceneBuildIndex, loadSceneMode, localPhysicsMode,
                true);
            tracker._sceneManagerWrapper.TrackSceneCountDummy = true;

            // call after scene is loaded
            tracker.ReplaceDummyScene(Op, sceneInfo.Path);

            tracker._loaded.Add(sceneInfo);
            if (Op == null || !tracker._tracked.TryGetValue(Op, out var state)) return;
            state.NotAllowedToStall = false;
            tracker._tracked[Op] = state;
        }

        public void Callback()
        {
            tracker.LoadingSceneCount--;
            if (loadSceneMode == LoadSceneMode.Additive)
                tracker._sceneManagerWrapper.LoadedSceneCountDummy++;
            else
                tracker._sceneManagerWrapper.LoadedSceneCountDummy = 1;
        }
    }

    private class AsyncSceneUnloadData(
        AsyncOperation asyncOperation,
        object options,
        SceneWrapper sceneWrapper,
        int realHandle,
        AsyncOperationTracker tracker) : IAsyncOperation
    {
        public AsyncOperation Op { get; } = asyncOperation;
        public int RealHandle { get; } = realHandle;
        public SceneWrapper SceneWrapper { get; } = sceneWrapper;

        public void Load()
        {
            tracker._logger.LogWarning(
                "async unload call: THIS OPERATION MIGHT BREAK THE GAME, scene unloading patch is using an unstable unity function, and it may fail");
            tracker._sceneManagerWrapper.TrackSceneCountDummy = false;
            tracker._sceneManagerWrapper.UnloadSceneAsync(SceneWrapper.Name, -1, options, true,
                out var success);
            tracker._sceneManagerWrapper.TrackSceneCountDummy = true;
            if (success) return;
            tracker._logger.LogError("async unload failed, prepare for game to maybe go nuts");
        }

        public void Callback()
        {
            tracker.LoadingSceneCount--;
        }
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

    private interface IAsyncOperation
    {
        /// <summary>
        /// Invoked when operation has to be completed, which means you call the equivalent non-async operation for this operation
        /// </summary>
        void Load();

        /// <summary>
        /// Invoked when callback is about to happen
        /// </summary>
        void Callback();

        AsyncOperation Op { get; }
    }
}