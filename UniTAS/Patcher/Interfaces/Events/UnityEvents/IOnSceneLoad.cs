using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;

namespace UniTAS.Patcher.Interfaces.Events.UnityEvents;

public interface IOnSceneLoad
{
    void OnSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode);
}