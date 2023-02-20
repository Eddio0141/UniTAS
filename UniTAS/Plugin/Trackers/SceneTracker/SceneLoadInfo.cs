namespace UniTAS.Plugin.Trackers.SceneTracker;

public class SceneLoadInfo
{
    public SceneInfo SceneInfo { get; }
    public bool Additive { get; }

    public SceneLoadInfo(SceneInfo sceneInfo, bool additive)
    {
        SceneInfo = sceneInfo;
        Additive = additive;
    }
}