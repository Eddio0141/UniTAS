using System;
using System.Collections.Generic;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Models.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Services.UnityAsyncOperationTracker;

public interface ISceneLoadTracker
{
    void AsyncSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, ref AsyncOperation asyncOperation);

    void NonAsyncSceneLoad(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode);

    void AsyncSceneUnload(ref AsyncOperation asyncOperation, Either<string, int> scene);

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

    int LoadingSceneCount { get; }

    /// <summary>
    /// The LoadingScene will provide dummy data for the fake instances, but once the scene is actually loaded,
    /// SceneWrapper will give real information for the fake instance to use 
    /// </summary>
    List<(object dummySceneStruct, IntPtr dummyScenePtr, AsyncOperationTracker.SceneInfo loadingScene, SceneWrapper actualSceneStruct)>
        LoadingScenes { get; }
}