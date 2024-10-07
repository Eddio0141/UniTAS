using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents;
using UniTAS.Patcher.Services.UnityEvents;

namespace UniTAS.Patcher.Implementations.UnityEvents;

[Singleton]
public class SceneLoad(IUpdateEvents updateEvent, IOnSceneLoad[] onSceneLoads) : ISceneLoadInvoke
{
    public void SceneLoadCall()
    {
        updateEvent.OnAwakeUnconditional += SceneLoadActual;
        updateEvent.OnStartUnconditional += SceneLoadActual;
        updateEvent.OnEnableUnconditional += SceneLoadActual;
        updateEvent.OnFixedUpdateUnconditional += SceneLoadActual;
    }

    private void SceneLoadActual()
    {
        updateEvent.OnAwakeUnconditional -= SceneLoadActual;
        updateEvent.OnStartUnconditional -= SceneLoadActual;
        updateEvent.OnEnableUnconditional -= SceneLoadActual;
        updateEvent.OnFixedUpdateUnconditional -= SceneLoadActual;

        foreach (var onSceneLoad in onSceneLoads)
        {
            onSceneLoad.OnSceneLoad();
        }
    }
}