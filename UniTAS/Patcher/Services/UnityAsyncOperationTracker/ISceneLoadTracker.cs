using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UnityEngine;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface ISceneLoadTracker
{
    void AsyncSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, AsyncOperation asyncOperation);

    void NonAsyncSceneLoad();

    void AsyncSceneUnload(AsyncOperation asyncOperation);

    void AllowSceneActivation(bool allow, AsyncOperation asyncOperation);

    /// <summary>
    /// Gets if done
    /// </summary>
    /// <param name="asyncOperation">The AsyncOperation to check</param>
    /// <param name="isDone">If IsDone is true or false, USE THIS NOT THE RETURN VALUE</param>
    /// <returns>True if is our tracked instance, otherwise it is some user created one</returns>
    bool IsDone(AsyncOperation asyncOperation, out bool isDone);

    /// <summary>
    /// Gets the progress
    /// </summary>
    /// <param name="asyncOperation">The AsyncOperation to check</param>
    /// <param name="progress">Progress of the AsyncOperation if it is tracked by UniTAS</param>
    /// <returns>True if is our tracked instance, otherwise it is some user created one</returns>
    bool Progress(AsyncOperation asyncOperation, out float progress);

    /// <summary>
    /// Gets allowSceneActivation state
    /// </summary>
    /// <param name="asyncOperation">The AsyncOperation to check</param>
    /// <param name="state">State of the AsyncOperation if it is tracked by UniTAS</param>
    /// <returns>True if is our tracked instance, otherwise it is some user created one</returns>
    bool GetAllowSceneActivation(AsyncOperation asyncOperation, out bool state);
}