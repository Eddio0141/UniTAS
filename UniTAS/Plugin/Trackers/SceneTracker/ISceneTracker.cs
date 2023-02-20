namespace UniTAS.Plugin.Trackers.SceneTracker;

public interface ISceneTracker
{
    void LoadScene(int sceneIndex, string sceneName, bool additive);
    void UnloadScene(int sceneIndex, string sceneName);
}