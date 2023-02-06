using System;

namespace UniTASPlugin.Trackers.SceneTracker;

public interface ILoadedSceneInfo
{
    void SubscribeOnSceneLoad(Action<SceneLoadInfo> callback);
    void SubscribeOnSceneUnload(Action<SceneInfo> callback);
}