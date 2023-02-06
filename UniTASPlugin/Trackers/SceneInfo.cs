namespace UniTASPlugin.Trackers;

public class SceneInfo
{
    public int SceneIndex { get; }
    public string SceneName { get; }

    public SceneInfo(int sceneIndex, string sceneName)
    {
        SceneIndex = sceneIndex;
        SceneName = sceneName;
    }
}