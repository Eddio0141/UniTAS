using System;
using System.Collections.Generic;
using UniTAS.Patcher.Shared;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.EventSubscribers;

namespace UniTAS.Plugin.Implementations;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class MonoBehEventInvoker : IMonoBehEventInvoker, IUpdateEvents
{
    public MonoBehEventInvoker(IEnumerable<IOnAwake> onAwakes, IEnumerable<IOnStart> onStarts,
        IEnumerable<IOnEnable> onEnables,
        IEnumerable<IOnPreUpdates> onPreUpdates, IEnumerable<IOnUpdate> onUpdates,
        IEnumerable<IOnFixedUpdate> onFixedUpdates, IEnumerable<IOnGUI> onGUIs)
    {
        foreach (var onAwake in onAwakes)
        {
            MonoBehaviourEvents.OnAwake += onAwake.Awake;
        }

        foreach (var onStart in onStarts)
        {
            MonoBehaviourEvents.OnStart += onStart.Start;
        }

        foreach (var onEnable in onEnables)
        {
            MonoBehaviourEvents.OnEnable += onEnable.OnEnable;
        }

        foreach (var onPreUpdate in onPreUpdates)
        {
            MonoBehaviourEvents.OnPreUpdate += onPreUpdate.PreUpdate;
        }

        foreach (var onUpdate in onUpdates)
        {
            MonoBehaviourEvents.OnUpdate += onUpdate.Update;
        }

        foreach (var onFixedUpdate in onFixedUpdates)
        {
            MonoBehaviourEvents.OnFixedUpdate += onFixedUpdate.FixedUpdate;
        }

        foreach (var onGui in onGUIs)
        {
            MonoBehaviourEvents.OnGUI += onGui.OnGUI;
        }

        MonoBehaviourEvents.OnGUI += () => OnGUIEvent?.Invoke();
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

    public event Action OnGUIEvent;
}