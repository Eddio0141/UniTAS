using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents;
using UniTAS.Patcher.Services.UnityEvents;

namespace UniTAS.Patcher.Implementations.UnityEvents;

[Singleton]
public class SceneLoad : ISceneLoadInvoke, IOnSceneLoadEvent
{
    private readonly IUpdateEvents _updateEvent;

    public SceneLoad(IUpdateEvents updateEvent, IOnSceneLoad[] onSceneLoads)
    {
        _updateEvent = updateEvent;
        foreach (var onSceneLoad in onSceneLoads)
        {
            OnSceneLoadEvent += onSceneLoad.OnSceneLoad;
        }
    }

    public void SceneLoadCall()
    {
        _updateEvent.OnAwakeUnconditional += SceneLoadActual;
        _updateEvent.OnStartUnconditional += SceneLoadActual;
        _updateEvent.OnEnableUnconditional += SceneLoadActual;
        _updateEvent.OnFixedUpdateUnconditional += SceneLoadActual;
    }

    private void SceneLoadActual()
    {
        _updateEvent.OnAwakeUnconditional -= SceneLoadActual;
        _updateEvent.OnStartUnconditional -= SceneLoadActual;
        _updateEvent.OnEnableUnconditional -= SceneLoadActual;
        _updateEvent.OnFixedUpdateUnconditional -= SceneLoadActual;

        OnSceneLoadEvent?.Invoke();
    }

    public event Action OnSceneLoadEvent;
}