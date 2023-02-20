using System;

namespace UniTAS.Plugin.Trackers.SceneTracker;

public interface ILoadedSceneInfo
{
    void SubscribeOnSceneLoad(Action<SceneLoadInfo> callback);
    void SubscribeOnSceneUnload(Action<SceneInfo> callback);
}