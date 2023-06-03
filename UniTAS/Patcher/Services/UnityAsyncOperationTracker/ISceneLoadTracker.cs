using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UnityEngine;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface ISceneLoadTracker
{
    void AsyncSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, AsyncOperation asyncOperation);

    void AllowSceneActivation(bool allow, AsyncOperation asyncOperation);
    bool IsStalling(AsyncOperation asyncOperation);
}