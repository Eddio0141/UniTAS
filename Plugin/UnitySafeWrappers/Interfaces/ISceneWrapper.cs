namespace UniTASPlugin.UnitySafeWrappers.Interfaces;

public interface ISceneWrapper
{
    void LoadSceneAsync(string sceneName, int sceneBuildIndex, object parameters, bool? isAdditive,
        bool mustCompleteNextFrame);

    void LoadScene(int buildIndex);
}