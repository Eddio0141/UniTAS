using System;
using System.Collections.Generic;

namespace UniTASPlugin.Trackers.SceneTracker;

// ReSharper disable once ClassNeverInstantiated.Global
public class SceneTracker : ISceneTracker, ILoadedSceneInfo
{
    private readonly List<SceneInfo> _loadedScenes = new();

    public void LoadScene(int sceneIndex, string sceneName, bool additive)
    {
        var sceneInfo = new SceneInfo(sceneIndex, sceneName);
        if (additive)
        {
            _loadedScenes.Add(sceneInfo);
        }
        else
        {
            _loadedScenes.Clear();
            _loadedScenes.Add(sceneInfo);
        }

        OnSceneLoad?.Invoke(new(sceneInfo, additive));
    }

    public void UnloadScene(int sceneIndex, string sceneName)
    {
        var sceneInfo = _loadedScenes.Find(x => x.SceneIndex == sceneIndex || x.SceneName == sceneName);
        if (sceneInfo == null) return;
        OnSceneUnload?.Invoke(sceneInfo);
        _loadedScenes.Remove(sceneInfo);
    }

    private event Action<SceneLoadInfo> OnSceneLoad;
    private event Action<SceneInfo> OnSceneUnload;

    public void SubscribeOnSceneLoad(Action<SceneLoadInfo> callback)
    {
        OnSceneLoad += callback;
    }
}