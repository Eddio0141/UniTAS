using System;

namespace UniTAS.Patcher.Interfaces.Events.UnityEvents;

public interface IOnSceneLoadEvent
{
    event Action OnSceneLoadEvent;
}