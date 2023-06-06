using System;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Services.EventSubscribers;

/// <summary>
/// An interface that you can use to subscribe to MonoBehaviour events.
/// </summary>
public interface IUpdateEvents
{
    event Action OnGUIEventUnconditional;
    event InputSystemEvents.InputUpdateCall OnInputUpdateActual;
}