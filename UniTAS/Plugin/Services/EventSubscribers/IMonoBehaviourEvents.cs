using System;

namespace UniTAS.Plugin.Services.EventSubscribers;

/// <summary>
/// An interface that you can use to subscribe to MonoBehaviour events.
/// </summary>
public interface IUpdateEvents
{
    event Action OnGUIEventUnconditional;
    event Action<bool> OnInputUpdateActual;
}