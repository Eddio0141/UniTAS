using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Models;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Services.UnityEvents;
#if TRACE
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Utils;
using UnityEngine;
#endif

namespace UniTAS.Patcher.Implementations.UnityEvents;

/// <summary>
/// Event calls for MonoBehaviour event methods
/// Unconditional will be called before Actual
/// Unconditional events are called even if MonoBehaviour is paused
/// Actual events are called only if MonoBehaviour is not paused
/// </summary>
public partial class UnityEvents : IUpdateEvents, IMonoBehEventInvoker
{
    public UnityEvents(IEnumerable<IOnAwakeUnconditional> onAwakesUnconditional,
        IEnumerable<IOnStartUnconditional> onStartsUnconditional,
        IEnumerable<IOnEnableUnconditional> onEnablesUnconditional,
        IEnumerable<IOnPreUpdateUnconditional> onPreUpdatesUnconditional,
        IEnumerable<IOnPreUpdateActual> onPreUpdatesActual,
        IEnumerable<IOnUpdateUnconditional> onUpdatesUnconditional,
        IEnumerable<IOnFixedUpdateUnconditional> onFixedUpdatesUnconditional,
        IEnumerable<IOnGUIUnconditional> onGUIsUnconditional,
        IEnumerable<IOnFixedUpdateActual> onFixedUpdatesActual,
        IEnumerable<IOnStartActual> onStartsActual,
        IEnumerable<IOnUpdateActual> onUpdatesActual,
        IEnumerable<IOnInputUpdateActual> onInputUpdatesActual,
        IEnumerable<IOnInputUpdateUnconditional> onInputUpdatesUnconditional,
        IEnumerable<IOnLateUpdateUnconditional> onLateUpdatesUnconditional,
        IEnumerable<IOnLastUpdateUnconditional> onLastUpdatesUnconditional,
        IEnumerable<IOnLastUpdateActual> onLastUpdatesActual)
    {
        _inputUpdatesUnconditional.Add((fixedUpdate, _) =>
        {
            if (fixedUpdate || _calledPreUpdate) return;
            _calledPreUpdate = true;
            InvokeCallOnPreUpdate();
        }, (int)CallbackPriority.PreUpdate);

        foreach (var onAwake in onAwakesUnconditional)
        {
            OnAwakeUnconditional += onAwake.AwakeUnconditional;
        }

        foreach (var onStart in onStartsUnconditional)
        {
            OnStartUnconditional += onStart.StartUnconditional;
        }

        foreach (var onEnable in onEnablesUnconditional)
        {
            OnEnableUnconditional += onEnable.OnEnableUnconditional;
        }

        foreach (var onPreUpdate in onPreUpdatesUnconditional)
        {
            OnPreUpdateUnconditional += onPreUpdate.PreUpdateUnconditional;
        }

        foreach (var onUpdate in onUpdatesUnconditional)
        {
            OnUpdateUnconditional += onUpdate.UpdateUnconditional;
        }

        foreach (var onFixedUpdate in onFixedUpdatesUnconditional)
        {
            OnFixedUpdateUnconditional += onFixedUpdate.FixedUpdateUnconditional;
        }

        foreach (var onGui in onGUIsUnconditional)
        {
            OnGUIUnconditional += onGui.OnGUIUnconditional;
        }

        foreach (var onPreUpdateActual in onPreUpdatesActual)
        {
            OnPreUpdateActual += onPreUpdateActual.PreUpdateActual;
        }

        foreach (var onFixedUpdateActual in onFixedUpdatesActual)
        {
            OnFixedUpdateActual += onFixedUpdateActual.FixedUpdateActual;
        }

        foreach (var onStartActual in onStartsActual)
        {
            OnStartActual += onStartActual.StartActual;
        }

        foreach (var onUpdateActual in onUpdatesActual)
        {
            OnUpdateActual += onUpdateActual.UpdateActual;
        }

        foreach (var onLateUpdateUnconditional in onLateUpdatesUnconditional)
        {
            OnLateUpdateUnconditional += onLateUpdateUnconditional.OnLateUpdateUnconditional;
        }

        foreach (var onLastUpdateUnconditional in onLastUpdatesUnconditional)
        {
            OnLastUpdateUnconditional += onLastUpdateUnconditional.OnLastUpdateUnconditional;
        }

        foreach (var onLastUpdateActual in onLastUpdatesActual)
        {
            OnLastUpdateActual += onLastUpdateActual.OnLastUpdateActual;
        }

        // input system events init
        foreach (var onInputUpdateActual in onInputUpdatesActual)
        {
            OnInputUpdateActual += (fixedUpdate, newInputSystemUpdateFixedUpdate) =>
                onInputUpdateActual.InputUpdateActual(fixedUpdate, newInputSystemUpdateFixedUpdate);
        }

        foreach (var onInputUpdateUnconditional in onInputUpdatesUnconditional)
        {
            OnInputUpdateUnconditional += (fixedUpdate, newInputSystemUpdateFixedUpdate) =>
                onInputUpdateUnconditional.InputUpdateUnconditional(fixedUpdate, newInputSystemUpdateFixedUpdate);
        }

        InputSystemEventsInit();
    }

    public event Action OnAwakeUnconditional
    {
        add => _awakesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => _awakesUnconditional.Remove(value);
    }

    public event Action OnAwakeActual
    {
        add => _awakesActual.Add(value, (int)CallbackPriority.Default);
        remove => _awakesActual.Remove(value);
    }

    public event Action OnStartUnconditional
    {
        add => _startsUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => _startsUnconditional.Remove(value);
    }

    public event Action OnStartActual
    {
        add => _startsActual.Add(value, (int)CallbackPriority.Default);
        remove => _startsActual.Remove(value);
    }

    public event Action OnEnableUnconditional
    {
        add => _enablesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => _enablesUnconditional.Remove(value);
    }

    public event Action OnEnableActual
    {
        add => _enablesActual.Add(value, (int)CallbackPriority.Default);
        remove => _enablesActual.Remove(value);
    }

    public event Action OnPreUpdateUnconditional
    {
        add => _preUpdatesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => _preUpdatesUnconditional.Remove(value);
    }

    public event Action OnPreUpdateActual
    {
        add => _preUpdatesActual.Add(value, (int)CallbackPriority.Default);
        remove => _preUpdatesActual.Remove(value);
    }

    public event Action OnUpdateUnconditional
    {
        add => _updatesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => _updatesUnconditional.Remove(value);
    }

    public event Action OnUpdateActual
    {
        add => _updatesActual.Add(value, (int)CallbackPriority.Default);
        remove => _updatesActual.Remove(value);
    }

    public event Action OnFixedUpdateUnconditional
    {
        add => _fixedUpdatesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => _fixedUpdatesUnconditional.Remove(value);
    }

    public event Action OnFixedUpdateActual
    {
        add => _fixedUpdatesActual.Add(value, (int)CallbackPriority.Default);
        remove => _fixedUpdatesActual.Remove(value);
    }

    public event Action OnGUIUnconditional
    {
        add => _guisUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => _guisUnconditional.Remove(value);
    }

    public event Action OnGUIActual
    {
        add => _guisActual.Add(value, (int)CallbackPriority.Default);
        remove => _guisActual.Remove(value);
    }

    public event Action OnLateUpdateUnconditional
    {
        add => _lateUpdatesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => _lateUpdatesUnconditional.Remove(value);
    }

    public event Action OnLateUpdateActual
    {
        add => _lateUpdatesActual.Add(value, (int)CallbackPriority.Default);
        remove => _lateUpdatesActual.Remove(value);
    }

    public event Action OnLastUpdateUnconditional
    {
        add => _lastUpdatesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => _lastUpdatesUnconditional.Remove(value);
    }

    public event Action OnLastUpdateActual
    {
        add => _lastUpdatesActual.Add(value, (int)CallbackPriority.Default);
        remove => _lastUpdatesActual.Remove(value);
    }

    public void AddPriorityCallback(CallbackUpdate callbackUpdate, Action callback, CallbackPriority priority)
    {
        var callbackList = callbackUpdate switch
        {
            CallbackUpdate.AwakeActual => _awakesActual,
            CallbackUpdate.AwakeUnconditional => _awakesUnconditional,
            CallbackUpdate.StartActual => _startsActual,
            CallbackUpdate.StartUnconditional => _startsUnconditional,
            CallbackUpdate.EnableActual => _enablesActual,
            CallbackUpdate.EnableUnconditional => _enablesUnconditional,
            CallbackUpdate.PreUpdateActual => _preUpdatesActual,
            CallbackUpdate.PreUpdateUnconditional => _preUpdatesUnconditional,
            CallbackUpdate.UpdateActual => _updatesActual,
            CallbackUpdate.UpdateUnconditional => _updatesUnconditional,
            CallbackUpdate.FixedUpdateActual => _fixedUpdatesActual,
            CallbackUpdate.FixedUpdateUnconditional => _fixedUpdatesUnconditional,
            CallbackUpdate.GUIActual => _guisActual,
            CallbackUpdate.GUIUnconditional => _guisUnconditional,
            CallbackUpdate.LastUpdateActual => _lastUpdatesActual,
            CallbackUpdate.LastUpdateUnconditional => _lastUpdatesUnconditional,
            _ => throw new ArgumentOutOfRangeException(nameof(callbackUpdate), callbackUpdate, null)
        };

        callbackList.Add(callback, (int)priority);
    }

    // if touching this, probably should rewrite but otherwise i ain't doing anything since its not like there's much to add
    private readonly PriorityList<Action> _awakesUnconditional = new();
    private readonly PriorityList<Action> _awakesActual = new();

    private readonly PriorityList<Action> _startsUnconditional = new();
    private readonly PriorityList<Action> _startsActual = new();

    private readonly PriorityList<Action> _enablesUnconditional = new();
    private readonly PriorityList<Action> _enablesActual = new();

    private readonly PriorityList<Action> _preUpdatesUnconditional = new();
    private readonly PriorityList<Action> _preUpdatesActual = new();

    private readonly PriorityList<Action> _updatesUnconditional = new();
    private readonly PriorityList<Action> _updatesActual = new();

    private readonly PriorityList<Action> _fixedUpdatesUnconditional = new();
    private readonly PriorityList<Action> _fixedUpdatesActual = new();

    private readonly PriorityList<Action> _guisUnconditional = new();
    private readonly PriorityList<Action> _guisActual = new();

    private readonly PriorityList<Action> _lateUpdatesUnconditional = new();
    private readonly PriorityList<Action> _lateUpdatesActual = new();

    private readonly PriorityList<Action> _lastUpdatesUnconditional = new();
    private readonly PriorityList<Action> _lastUpdatesActual = new();

    private bool _updated;
    private bool _calledFixedUpdate;
    private bool _calledPreUpdate;

    public void InvokeLastUpdate()
    {
        for (var i = 0; i < _lastUpdatesUnconditional.Count; i++)
        {
            _lastUpdatesUnconditional[i]();
        }

        for (var i = 0; i < _lastUpdatesActual.Count; i++)
        {
            var lastUpdate = _lastUpdatesActual[i];
            if (Utils.MonoBehaviourController.PausedExecution || Utils.MonoBehaviourController.PausedUpdate) continue;
            lastUpdate();
        }
    }

    // calls awake before any other script
    public void InvokeAwake()
    {
        for (var i = 0; i < _awakesUnconditional.Count; i++)
        {
            _awakesUnconditional[i]();
        }

        for (var i = 0; i < _awakesActual.Count; i++)
        {
            var awake = _awakesActual[i];
            if (!Utils.MonoBehaviourController.PausedExecution)
                awake();
        }
    }

    // calls onEnable before any other script
    public void InvokeOnEnable()
    {
        for (var i = 0; i < _enablesUnconditional.Count; i++)
        {
            _enablesUnconditional[i]();
        }

        for (var i = 0; i < _enablesActual.Count; i++)
        {
            var enable = _enablesActual[i];
            if (!Utils.MonoBehaviourController.PausedExecution)
                enable();
        }
    }

    // calls start before any other script
    public void InvokeStart()
    {
        for (var i = 0; i < _startsUnconditional.Count; i++)
        {
            _startsUnconditional[i]();
        }

        for (var i = 0; i < _startsActual.Count; i++)
        {
            var start = _startsActual[i];
            if (!Utils.MonoBehaviourController.PausedExecution)
                start();
        }
    }

#if TRACE
    private IPatchReverseInvoker _patchReverseInvoker;
#endif

    public void InvokeUpdate()
    {
        if (_updated) return;
        _updated = true;

        _calledFixedUpdate = false;

#if TRACE
        _patchReverseInvoker ??= ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();
        StaticLogger.Trace(
            $"InvokeUpdate, time: {_patchReverseInvoker.Invoke(() => Time.time)}");
#endif

        if (!_calledPreUpdate)
        {
            _calledPreUpdate = true;
            InvokeCallOnPreUpdate();
        }

        for (var i = 0; i < _updatesUnconditional.Count; i++)
        {
            _updatesUnconditional[i]();
        }

        for (var i = 0; i < _updatesActual.Count; i++)
        {
            var update = _updatesActual[i];
            if (Utils.MonoBehaviourController.PausedExecution || Utils.MonoBehaviourController.PausedUpdate) continue;
            update();
        }
    }

    // right now I don't call this update before other scripts so I don't need to check if it was already called
    public void InvokeLateUpdate()
    {
        _updated = false;
        _calledPreUpdate = false;

        for (var i = 0; i < _lateUpdatesUnconditional.Count; i++)
        {
            _lateUpdatesUnconditional[i]();
        }

        for (var i = 0; i < _lateUpdatesActual.Count; i++)
        {
            var lateUpdate = _lateUpdatesActual[i];
            if (Utils.MonoBehaviourController.PausedExecution || Utils.MonoBehaviourController.PausedUpdate) continue;
            lateUpdate();
        }
    }

    public void InvokeFixedUpdate()
    {
        if (_calledFixedUpdate) return;
        _calledFixedUpdate = true;

#if TRACE
        _patchReverseInvoker ??= ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();
        StaticLogger.Trace($"InvokeFixedUpdate, time: {_patchReverseInvoker.Invoke(() => Time.time)}");
#endif

        InvokeCallOnPreUpdate();

        for (var i = 0; i < _fixedUpdatesUnconditional.Count; i++)
        {
            _fixedUpdatesUnconditional[i]();
        }

        for (var i = 0; i < _fixedUpdatesActual.Count; i++)
        {
            var fixedUpdate = _fixedUpdatesActual[i];
            if (!Utils.MonoBehaviourController.PausedExecution)
                fixedUpdate();
        }
    }

    public void InvokeOnGUI()
    {
        // currently, this doesn't get called before other scripts
        for (var i = 0; i < _guisUnconditional.Count; i++)
        {
            _guisUnconditional[i]();
        }

        for (var i = 0; i < _guisActual.Count; i++)
        {
            var gui = _guisActual[i];
            if (!Utils.MonoBehaviourController.PausedExecution)
                gui();
        }
    }

    private void InvokeCallOnPreUpdate()
    {
        for (var i = 0; i < _preUpdatesUnconditional.Count; i++)
        {
            _preUpdatesUnconditional[i]();
        }

        for (var i = 0; i < _preUpdatesActual.Count; i++)
        {
            var preUpdate = _preUpdatesActual[i];
            if (!Utils.MonoBehaviourController.PausedExecution)
                preUpdate();
        }
    }
}