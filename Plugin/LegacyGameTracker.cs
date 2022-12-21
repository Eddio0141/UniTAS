using System.Collections.Generic;
using System.Linq;
using UniTASPlugin.LegacySafeWrappers;
using UnityEngine;

// ReSharper disable StringLiteralTypo

namespace UniTASPlugin;

public static class GameTracker
{
    public static void LateUpdate()
    {
        foreach (var scene in asyncSceneLoads)
        {
            Plugin.Log.LogDebug($"force loading scene, name: {scene.sceneName} {scene.sceneBuildIndex}");
            SceneHelper.LoadSceneAsyncNameIndexInternal(scene.sceneName, scene.sceneBuildIndex, scene.parameters,
                scene.isAdditive, true);
        }

        asyncSceneLoads.Clear();
    }

    private struct AsyncSceneLoadData
    {
        public string sceneName;
        public int sceneBuildIndex;
        public object parameters;
        public bool? isAdditive;
        public ulong UID;

        public AsyncSceneLoadData(string sceneName, int sceneBuildIndex, object parameters, bool? isAdditive,
            AsyncOperationWrap wrap)
        {
            this.sceneName = sceneName;
            this.sceneBuildIndex = sceneBuildIndex;
            this.parameters = parameters;
            this.isAdditive = isAdditive;
            UID = wrap.UID;
        }
    }

    private static readonly List<AsyncSceneLoadData> asyncSceneLoads = new();
    private static readonly List<AsyncSceneLoadData> asyncSceneLoadsStall = new();

    public static void AsyncSceneLoad(string sceneName, int sceneBuildIndex, object parameters, bool? isAdditive,
        AsyncOperationWrap wrap)
    {
        asyncSceneLoads.Add(new(sceneName, sceneBuildIndex, parameters, isAdditive, wrap));
    }

    public static void AllowSceneActivation(bool allow, AsyncOperation instance)
    {
        var wrap = new AsyncOperationWrap(instance);
        var uid = wrap.UID;
        Plugin.Log.LogDebug($"allow scene activation {allow} for UID {uid}");
        if (wrap.InstantiatedByUnity)
        {
            Plugin.Log.LogError("AsyncOperation UID is 0, this should not happen");
            return;
        }

        if (allow)
        {
            var sceneToLoadIndex = asyncSceneLoadsStall.FindIndex(s => s.UID == uid);
            if (sceneToLoadIndex < 0)
                return;
            var sceneToLoad = asyncSceneLoadsStall[sceneToLoadIndex];
            asyncSceneLoadsStall.RemoveAt(sceneToLoadIndex);
            SceneHelper.LoadSceneAsyncNameIndexInternal(sceneToLoad.sceneName, sceneToLoad.sceneBuildIndex,
                sceneToLoad.parameters, sceneToLoad.isAdditive, true);
            Plugin.Log.LogDebug(
                $"force loading scene, name: {sceneToLoad.sceneName} build index: {sceneToLoad.sceneBuildIndex}");
        }
        else
        {
            var asyncSceneLoadsIndex = asyncSceneLoads.FindIndex(s => s.UID == uid);
            if (asyncSceneLoadsIndex < 0)
                return;
            var scene = asyncSceneLoads[asyncSceneLoadsIndex];
            asyncSceneLoads.RemoveAt(asyncSceneLoadsIndex);
            asyncSceneLoadsStall.Add(scene);
            Plugin.Log.LogDebug("Added scene to stall list");
        }
    }

    public static void AsyncOperationFinalize(ulong uid)
    {
        var removeIndex = asyncSceneLoadsStall.FindIndex(a => a.UID == uid);
        if (removeIndex < 0)
            return;
        asyncSceneLoadsStall.RemoveAt(removeIndex);
    }

    public static bool GetSceneActivation(AsyncOperation instance)
    {
        var uid = new AsyncOperationWrap(instance).UID;
        return uid != 0 && asyncSceneLoadsStall.Any(a => a.UID == uid);
    }

    public static bool IsStallingInstance(AsyncOperation instance)
    {
        var uid = new AsyncOperationWrap(instance).UID;
        return uid != 0 && asyncSceneLoadsStall.Any(x => x.UID == uid);
    }
}