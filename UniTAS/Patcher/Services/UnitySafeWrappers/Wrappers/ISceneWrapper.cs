using UniTAS.Patcher.Models.UnitySafeWrappers.SceneManagement;

namespace UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;

public interface ISceneWrapper
{
    void LoadSceneAsync(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, bool mustCompleteNextFrame);

    void LoadScene(int buildIndex);
    void LoadScene(string name);

    int TotalSceneCount { get; }

    int ActiveSceneIndex { get; }

    string ActiveSceneName { get; }

    // not really an actual call, but to keep track of stuff
    int SceneCount { get; set; }
    /// <summary>
    /// Disabling this would not update SceneCount from any of the LoadScene functions here
    /// </summary>
    bool TrackSceneCount { get; set; }
}