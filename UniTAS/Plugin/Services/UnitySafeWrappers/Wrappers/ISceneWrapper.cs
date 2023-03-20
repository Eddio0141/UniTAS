using UniTAS.Plugin.Models.UnitySafeWrappers.SceneManagement;

namespace UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;

public interface ISceneWrapper
{
    void LoadSceneAsync(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, bool mustCompleteNextFrame);

    void LoadScene(int buildIndex);

    int TotalSceneCount { get; }

    int ActiveSceneIndex { get; }
    string ActiveSceneName { get; }
}