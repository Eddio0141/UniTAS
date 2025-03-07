using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton(timing: RegisterTiming.Entry)]
[ExcludeRegisterIfTesting]
public class UpdateInvokeOffset : IUpdateInvokeOffset, IUnityEventPriority, IOnUpdateUnconditional,
    IOnLateUpdateUnconditional
{
    public double Offset { get; private set; }

    private bool _updated;

    public void UpdateUnconditional()
    {
        UpdateOffset();
    }

    public void OnLateUpdateUnconditional()
    {
        _updated = false;
    }

    private void UpdateOffset()
    {
        if (_updated) return;
        _updated = true;

        Offset += Time.deltaTime;
        Offset %= Time.fixedDeltaTime;
        StaticLogger.Trace($"New update offset: {Offset}, dt: {Time.deltaTime}");
    }

    public Dictionary<CallbackUpdate, CallbackPriority> Priorities { get; } = new()
    {
        { CallbackUpdate.UpdateUnconditional, CallbackPriority.UpdateInvokeOffset },
    };
}