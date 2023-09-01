using System;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Services.EventSubscribers;

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
    event Action OnGUIEventUnconditional;
    event Action OnLastUpdateUnconditional;
    event Action OnLastUpdateActual;
    event Action OnPreUpdatesActual;
    event Action OnPreUpdatesUnconditional;
    event InputSystemEvents.InputUpdateCall OnInputUpdateActual;
    event InputSystemEvents.InputUpdateCall OnInputUpdateUnconditional;

    void AddPriorityCallback(CallbackUpdate callbackUpdate, Action callback, CallbackPriority priority);

    void AddPriorityCallback(CallbackInputUpdate callbackUpdate, InputSystemEvents.InputUpdateCall callback,
        CallbackPriority priority);
}