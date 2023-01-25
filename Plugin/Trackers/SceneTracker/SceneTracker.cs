using System.Collections.Generic;

namespace UniTASPlugin.Trackers.SceneTracker;

// ReSharper disable once ClassNeverInstantiated.Global
public class SceneTracker : ISceneTracker
{
    private readonly List<SceneInfo> _loadedScenes = new();

    public void LoadScene(int sceneIndex, string sceneName, bool additive)
    {
        if (additive)
        {
            _loadedScenes.Add(new(sceneIndex, sceneName));
        }
        else
        {
            _loadedScenes.Clear();
            _loadedScenes.Add(new(sceneIndex, sceneName));
        }
    }

    public void UnloadScene(int sceneIndex, string sceneName)
    {
        _loadedScenes.RemoveAll(x => x.SceneIndex == sceneIndex || x.SceneName == sceneName);
    }
}