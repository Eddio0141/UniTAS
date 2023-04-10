using System;
using System.Collections.Generic;
using UniTAS.Patcher.Shared;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.EventSubscribers;

namespace UniTAS.Plugin.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class MonoBehEventInvoker : IMonoBehEventInvoker, IUpdateEvents
{
    public MonoBehEventInvoker(IEnumerable<IOnAwakeUnconditional> onAwakesUnconditional,
        IEnumerable<IOnStartUnconditional> onStartsUnconditional,
        IEnumerable<IOnEnableUnconditional> onEnablesUnconditional,
        IEnumerable<IOnPreUpdatesUnconditional> onPreUpdatesUnconditional,
        IEnumerable<IOnUpdateUnconditional> onUpdatesUnconditional,
        IEnumerable<IOnFixedUpdateUnconditional> onFixedUpdatesUnconditional,
        IEnumerable<IOnGUIUnconditional> onGUIsUnconditional,
        IEnumerable<IOnPreUpdatesActual> onPreUpdatesActual,
        IEnumerable<IOnFixedUpdateActual> onFixedUpdatesActual,
        IEnumerable<IOnStartActual> onStartsActual,
        IEnumerable<IOnUpdateActual> onUpdatesActual)
    {
        foreach (var onAwake in onAwakesUnconditional)
        {
            MonoBehaviourEvents.OnAwakeUnconditional += onAwake.AwakeUnconditional;
        }

        foreach (var onStart in onStartsUnconditional)
        {
            MonoBehaviourEvents.OnStartUnconditional += onStart.StartUnconditional;
        }

        foreach (var onEnable in onEnablesUnconditional)
        {
            MonoBehaviourEvents.OnEnableUnconditional += onEnable.OnEnableUnconditional;
        }

        foreach (var onPreUpdate in onPreUpdatesUnconditional)
        {
            MonoBehaviourEvents.OnPreUpdateUnconditional += onPreUpdate.PreUpdateUnconditional;
        }

        foreach (var onUpdate in onUpdatesUnconditional)
        {
            MonoBehaviourEvents.OnUpdateUnconditional += onUpdate.UpdateUnconditional;
        }

        foreach (var onFixedUpdate in onFixedUpdatesUnconditional)
        {
            MonoBehaviourEvents.OnFixedUpdateUnconditional += onFixedUpdate.FixedUpdateUnconditional;
        }

        foreach (var onGui in onGUIsUnconditional)
        {
            MonoBehaviourEvents.OnGUIUnconditional += onGui.OnGUIUnconditional;
        }

        foreach (var onPreUpdateActual in onPreUpdatesActual)
        {
            MonoBehaviourEvents.OnPreUpdateActual += onPreUpdateActual.PreUpdateActual;
        }

        foreach (var onFixedUpdateActual in onFixedUpdatesActual)
        {
            MonoBehaviourEvents.OnFixedUpdateActual += onFixedUpdateActual.FixedUpdateActual;
        }

        foreach (var onStartActual in onStartsActual)
        {
            MonoBehaviourEvents.OnStartActual += onStartActual.StartActual;
        }

        foreach (var onUpdateActual in onUpdatesActual)
        {
            MonoBehaviourEvents.OnUpdateActual += onUpdateActual.UpdateActual;
        }

        MonoBehaviourEvents.OnGUIUnconditional += () => OnGUIEventUnconditional?.Invoke();
    }

    public void Update()
    {
        MonoBehaviourEvents.InvokeUpdate();
    }

    public void FixedUpdate()
    {
        MonoBehaviourEvents.InvokeFixedUpdate();
    }

    public void OnGUI()
    {
        MonoBehaviourEvents.InvokeOnGUI();
    }

    public void LateUpdate()
    {
        MonoBehaviourEvents.InvokeLateUpdate();
    }

    public event Action OnGUIEventUnconditional;
}