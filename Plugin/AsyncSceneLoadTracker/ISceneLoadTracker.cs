namespace UniTASPlugin.AsyncSceneLoadTracker;

public interface ISceneLoadTracker
{
    void AsyncSceneLoad(string sceneName, int sceneBuildIndex, object parameters, bool? isAdditive,
        object asyncOperation);

    void AllowSceneActivation(bool allow, object asyncOperation);
    void AsyncOperationDestruction(object asyncOperation);
    bool IsStalling(object asyncOperation);
}