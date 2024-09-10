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
}