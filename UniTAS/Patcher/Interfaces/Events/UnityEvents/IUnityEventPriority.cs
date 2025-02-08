using System.Collections.Generic;
using UniTAS.Patcher.Models.EventSubscribers;

namespace UniTAS.Patcher.Interfaces.Events.UnityEvents;

public interface IUnityEventPriority
{
    Dictionary<CallbackUpdate, CallbackPriority> Priorities { get; }
}