using System.Collections.Generic;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Models.Utils;

namespace UniTAS.Patcher.Interfaces.Events.UnityEvents;

public interface IUnityEventPriority
{
    Dictionary<Either<CallbackUpdate, CallbackInputUpdate>, CallbackPriority> Priorities { get; }
}