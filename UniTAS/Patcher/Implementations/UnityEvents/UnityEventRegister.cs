using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Services.UnityEvents;

namespace UniTAS.Patcher.Implementations.UnityEvents;

[Register]
[ForceInstantiate]
public class UnityEventRegister
{
    public UnityEventRegister(IEnumerable<IOnAwakeUnconditional> onAwakesUnconditional,
        IEnumerable<IOnAwakeActual> onAwakeActual,
        IEnumerable<IOnStartUnconditional> onStartsUnconditional,
        IEnumerable<IOnEnableUnconditional> onEnablesUnconditional,
        IEnumerable<IOnUpdateUnconditional> onUpdatesUnconditional,
        IEnumerable<IOnFixedUpdateUnconditional> onFixedUpdatesUnconditional,
        IEnumerable<IOnGUIUnconditional> onGuisUnconditional,
        IEnumerable<IOnFixedUpdateActual> onFixedUpdatesActual,
        IEnumerable<IOnStartActual> onStartsActual,
        IEnumerable<IOnUpdateActual> onUpdatesActual,
        IEnumerable<IOnLateUpdateUnconditional> onLateUpdatesUnconditional,
        IEnumerable<IOnLastUpdateUnconditional> onLastUpdatesUnconditional,
        IEnumerable<IOnLastUpdateActual> onLastUpdatesActual,
        IEnumerable<IOnEndOfFrameActual> onEndOfFrameActual, IUpdateEvents updateEvents)
    {
        foreach (var onAwake in onAwakesUnconditional)
        {
            updateEvents.RegisterMethod(onAwake, onAwake.AwakeUnconditional, CallbackUpdate.AwakeUnconditional);
        }

        foreach (var onAwake in onAwakeActual)
        {
            updateEvents.RegisterMethod(onAwake, onAwake.AwakeActual, CallbackUpdate.AwakeActual);
        }

        foreach (var onStart in onStartsUnconditional)
        {
            updateEvents.RegisterMethod(onStart, onStart.StartUnconditional, CallbackUpdate.StartUnconditional);
        }

        foreach (var onEnable in onEnablesUnconditional)
        {
            updateEvents.RegisterMethod(onEnable, onEnable.OnEnableUnconditional, CallbackUpdate.EnableUnconditional);
        }

        foreach (var onUpdate in onUpdatesUnconditional)
        {
            updateEvents.RegisterMethod(onUpdate, onUpdate.UpdateUnconditional, CallbackUpdate.UpdateUnconditional);
        }

        foreach (var onFixedUpdate in onFixedUpdatesUnconditional)
        {
            updateEvents.RegisterMethod(onFixedUpdate, onFixedUpdate.FixedUpdateUnconditional,
                CallbackUpdate.FixedUpdateUnconditional);
        }

        foreach (var onGui in onGuisUnconditional)
        {
            updateEvents.RegisterMethod(onGui, onGui.OnGUIUnconditional, CallbackUpdate.GUIUnconditional);
        }

        foreach (var onFixedUpdateActual in onFixedUpdatesActual)
        {
            updateEvents.RegisterMethod(onFixedUpdateActual, onFixedUpdateActual.FixedUpdateActual,
                CallbackUpdate.FixedUpdateActual);
        }

        foreach (var onStartActual in onStartsActual)
        {
            updateEvents.RegisterMethod(onStartActual, onStartActual.StartActual, CallbackUpdate.StartActual);
        }

        foreach (var onUpdateActual in onUpdatesActual)
        {
            updateEvents.RegisterMethod(onUpdateActual, onUpdateActual.UpdateActual, CallbackUpdate.UpdateActual);
        }

        foreach (var onLateUpdateUnconditional in onLateUpdatesUnconditional)
        {
            updateEvents.RegisterMethod(onLateUpdateUnconditional, onLateUpdateUnconditional.OnLateUpdateUnconditional,
                CallbackUpdate.LateUpdateUnconditional);
        }

        foreach (var onLastUpdateUnconditional in onLastUpdatesUnconditional)
        {
            updateEvents.RegisterMethod(onLastUpdateUnconditional, onLastUpdateUnconditional.OnLastUpdateUnconditional,
                CallbackUpdate.LastUpdateUnconditional);
        }

        foreach (var onLastUpdateActual in onLastUpdatesActual)
        {
            updateEvents.RegisterMethod(onLastUpdateActual, onLastUpdateActual.OnLastUpdateActual,
                CallbackUpdate.LastUpdateActual);
        }

        foreach (var endOfFrameActual in onEndOfFrameActual)
        {
            updateEvents.RegisterMethod(endOfFrameActual, endOfFrameActual.OnEndOfFrame,
                CallbackUpdate.EndOfFrameActual);
        }
    }
}