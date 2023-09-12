using System;
using UniTAS.Patcher.Models.EventSubscribers;

namespace UniTAS.Patcher.Services.UnityEvents;

/// <summary>
/// An interface that you can use to subscribe to MonoBehaviour events.
/// </summary>
public interface IUpdateEvents
{
    event Action OnAwakeActual;
    event Action OnAwakeUnconditional;
    event Action OnStartActual;
    event Action OnFixedUpdateActual;
    event Action OnFixedUpdateUnconditional;
    event Action OnUpdateActual;
    event Action OnUpdateUnconditional;
    event Action OnGUIUnconditional;
    event Action OnLateUpdateActual;
    event Action OnLateUpdateUnconditional;
    event Action OnLastUpdateUnconditional;
    event Action OnLastUpdateActual;
    event Action OnPreUpdateActual;
    event Action OnPreUpdateUnconditional;
    event InputUpdateCall OnInputUpdateActual;
    event InputUpdateCall OnInputUpdateUnconditional;

    void AddPriorityCallback(CallbackUpdate callbackUpdate, Action callback, CallbackPriority priority);

    void AddPriorityCallback(CallbackInputUpdate callbackUpdate, InputUpdateCall callback, CallbackPriority priority);

    public delegate void InputUpdateCall(bool fixedUpdate, bool newInputSystemUpdate);
}