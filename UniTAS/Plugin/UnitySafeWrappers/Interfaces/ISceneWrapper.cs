using UniTAS.Plugin.UnitySafeWrappers.Wrappers;

namespace UniTAS.Plugin.UnitySafeWrappers.Interfaces;

public interface ISceneWrapper
{
    void LoadSceneAsync(string sceneName, int sceneBuildIndex, LoadSceneMode loadSceneMode,
        LocalPhysicsMode localPhysicsMode, bool mustCompleteNextFrame);

    void LoadScene(int buildIndex);

    int TotalSceneCount { get; }

    int ActiveSceneIndex { get; }
    string ActiveSceneName { get; }
}