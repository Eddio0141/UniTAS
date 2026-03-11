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

    void AsyncSceneUnload(ref AsyncOperation asyncOperation, Either<string, int> scene, object options);

    void AsyncSceneUnload(ref AsyncOperation asyncOperation, object scene, object options);

    void AsyncSceneUnload(out IntPtr result, object scene, object options);

    /// <summary>
    /// Attempts to convert to UniTAS managed AsyncOperation
    /// This is for UnityEngine.AsyncOperation.BindingsMarshaller.ConvertToManaged where passing in pointer for
    /// AsyncOperation will convert it to managed UniTAS version (if it exists)
    /// </summary>
    /// <returns>Instance of AsyncOperation or null if not found</returns>
    AsyncOperation ConvertToManaged(IntPtr ptr);

    void AllowSceneActivation(bool allow, AsyncOperation asyncOperation);

    /// <summary>
    /// Gets allowSceneActivation state
    /// </summary>
    /// <param name="asyncOperation">The AsyncOperation to check</param>
    /// <param name="state">State of the AsyncOperation if it is tracked by UniTAS</param>
    /// <returns>True if is our tracked instance, otherwise it is some user created one</returns>
    bool GetAllowSceneActivation(AsyncOperation asyncOperation, out bool state);

    int LoadingSceneCount { get; }

    List<(AsyncOperationTracker.DummyScene dummyScene, SceneWrapper actualScene)> DummyScenes { get; }

    /// <summary>
    /// The LoadingScene will provide dummy data for the fake instances, but once the scene is actually loaded,
    /// SceneWrapper will give real information for the fake instance to use
    /// </summary>
    List<AsyncOperationTracker.DummyScene> LoadingScenes { get; }
}
