using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton(timing: RegisterTiming.Entry)]
public class UpdateInvokeOffset : IUpdateInvokeOffset
{
    public double Offset { get; private set; }

    private bool _updated;

    public UpdateInvokeOffset(IUpdateEvents updateEvents)
    {
        // to make sure this is called before any other update events, we register on both Update and InputUpdate
        updateEvents.AddPriorityCallback(CallbackUpdate.UpdateUnconditional, UpdateOffset,
            CallbackPriority.UpdateInvokeOffset);
        updateEvents.AddPriorityCallback(CallbackInputUpdate.InputUpdateUnconditional, (fixedUpdate, _) =>
        {
            if (!fixedUpdate)
            {
                UpdateOffset();
            }
        }, CallbackPriority.UpdateInvokeOffset);
        updateEvents.OnLateUpdateUnconditional += () => _updated = false;
    }

    private void UpdateOffset()
    {
        if (_updated) return;
        _updated = true;

        Offset += Time.deltaTime;
        Offset %= Time.fixedDeltaTime;
        StaticLogger.Trace($"New update offset: {Offset}, dt: {Time.deltaTime}");
    }
}