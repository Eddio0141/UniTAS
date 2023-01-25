namespace UniTASPlugin.Trackers.SceneIndexNameTracker;

public interface ISceneIndexName
{
    int? GetSceneIndex(string sceneName);
    string GetSceneName(int sceneIndex);
}