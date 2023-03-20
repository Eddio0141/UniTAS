using UniTAS.Plugin.UnitySafeWrappers.Wrappers;

namespace UniTAS.Plugin.Services.UnityAsyncOperationTracker;

public interface ISceneLoadTracker
{
    void AsyncSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, object asyncOperation);

    void AllowSceneActivation(bool allow, object asyncOperation);
    void AsyncOperationDestruction(object asyncOperation);
    bool IsStalling(object asyncOperation);
}