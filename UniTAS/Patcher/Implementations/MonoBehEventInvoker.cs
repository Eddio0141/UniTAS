using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.EventSubscribers;
using UniTAS.Patcher.StaticServices;

namespace UniTAS.Patcher.Implementations;

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
        IEnumerable<IOnUpdateActual> onUpdatesActual,
        IEnumerable<IOnInputUpdateActual> onInputUpdatesActual,
        IEnumerable<IOnInputUpdateUnconditional> onInputUpdatesUnconditional)
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

        foreach (var onInputUpdateActual in onInputUpdatesActual)
        {
            InputSystemEvents.OnInputUpdateActual += (fixedUpdate, newInputSystemUpdateFixedUpdate) =>
                onInputUpdateActual.InputUpdateActual(fixedUpdate, newInputSystemUpdateFixedUpdate);
        }

        foreach (var onInputUpdateUnconditional in onInputUpdatesUnconditional)
        {
            InputSystemEvents.OnInputUpdateUnconditional += (fixedUpdate, newInputSystemUpdateFixedUpdate) =>
                onInputUpdateUnconditional.InputUpdateUnconditional(fixedUpdate, newInputSystemUpdateFixedUpdate);
        }
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

    public event Action OnGUIEventUnconditional
    {
        add => MonoBehaviourEvents.OnGUIUnconditional += value;
        remove => MonoBehaviourEvents.OnGUIUnconditional -= value;
    }

    public event InputSystemEvents.InputUpdateCall OnInputUpdateActual
    {
        add => InputSystemEvents.OnInputUpdateActual += value;
        remove => InputSystemEvents.OnInputUpdateActual -= value;
    }
}