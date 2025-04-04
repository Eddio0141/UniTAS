using JetBrains.Annotations;
using UniTAS.Patcher.Implementations.UnitySafeWrappers.SceneManagement;
using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;

namespace UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;

public interface ISceneManagerWrapper
{
    void LoadSceneAsync(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, bool mustCompleteNextFrame);

    void UnloadSceneAsync(string sceneName, int sceneBuildIndex, object options, bool immediate, out bool success);

    void LoadScene(int buildIndex);
    void LoadScene(string name);

    int TotalSceneCount { get; }

    int ActiveSceneIndex { get; }

    [UsedImplicitly] // for test runner
    string ActiveSceneName { get; }

    // not really an actual call, but to keep track of stuff
    int LoadedSceneCountDummy { get; set; }

    /// <summary>
    /// Disabling this would not update SceneCount from any of the LoadScene functions here
    /// </summary>
    bool TrackSceneCountDummy { get; set; }

    int SceneCount { get; }
    SceneWrapper GetSceneAt(int index);
}