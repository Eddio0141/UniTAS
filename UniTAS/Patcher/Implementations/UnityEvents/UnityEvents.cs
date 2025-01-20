using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Models.Utils;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.UnityEvents;
using UnityEngine;
#if TRACE
using UniTAS.Patcher.Utils;
#endif

namespace UniTAS.Patcher.Implementations.UnityEvents;

/// <summary>
/// Event calls for MonoBehaviour event methods
/// Unconditional will be called before Actual
/// Unconditional events are called even if MonoBehaviour is paused
/// Actual events are called only if MonoBehaviour is not paused
/// </summary>
[Singleton(timing: RegisterTiming.Entry)]
public partial class UnityEvents : IUpdateEvents, IMonoBehEventInvoker, IInputEventInvoker
{
    private readonly IPatchReverseInvoker _patchReverseInvoker;

    public UnityEvents(IEnumerable<IOnAwakeUnconditional> onAwakesUnconditional,
        IEnumerable<IOnAwakeActual> onAwakeActual,
        IEnumerable<IOnStartUnconditional> onStartsUnconditional,
        IEnumerable<IOnEnableUnconditional> onEnablesUnconditional,
        IEnumerable<IOnPreUpdateUnconditional> onPreUpdatesUnconditional,
        IEnumerable<IOnPreUpdateActual> onPreUpdatesActual,
        IEnumerable<IOnUpdateUnconditional> onUpdatesUnconditional,
        IEnumerable<IOnFixedUpdateUnconditional> onFixedUpdatesUnconditional,
        IEnumerable<IOnGUIUnconditional> onGuisUnconditional,
        IEnumerable<IOnFixedUpdateActual> onFixedUpdatesActual,
        IEnumerable<IOnStartActual> onStartsActual,
        IEnumerable<IOnUpdateActual> onUpdatesActual,
        IEnumerable<IOnInputUpdateActual> onInputUpdatesActual,
        IEnumerable<IOnInputUpdateUnconditional> onInputUpdatesUnconditional,
        IEnumerable<IOnLateUpdateUnconditional> onLateUpdatesUnconditional,
        IEnumerable<IOnLastUpdateUnconditional> onLastUpdatesUnconditional,
        IEnumerable<IOnLastUpdateActual> onLastUpdatesActual,
        IEnumerable<IOnEndOfFrameActual> onEndOfFrameActual,
        IGameRestart gameRestart,
        IInputSystemState newInputSystemExists, IMonoBehaviourController monoBehaviourController,
        IPatchReverseInvoker patchReverseInvoker
    )
    {
        _patchReverseInvoker = patchReverseInvoker;
        _newInputSystemExists = newInputSystemExists;
        _monoBehaviourController = monoBehaviourController;

        gameRestart.OnGameRestart += OnGameRestart;

        _inputUpdatesUnconditional.Add((fixedUpdate, _) =>
        {
            if (fixedUpdate || _calledPreUpdate) return;
            _calledPreUpdate = true;
            InvokeCallOnPreUpdate();
        }, (int)CallbackPriority.PreUpdate);

        foreach (var onAwake in onAwakesUnconditional)
        {
            RegisterMethod(onAwake, onAwake.AwakeUnconditional, CallbackUpdate.AwakeUnconditional);
        }

        foreach (var onAwake in onAwakeActual)
        {
            RegisterMethod(onAwake, onAwake.AwakeActual, CallbackUpdate.AwakeActual);
        }

        foreach (var onStart in onStartsUnconditional)
        {
            RegisterMethod(onStart, onStart.StartUnconditional, CallbackUpdate.StartUnconditional);
        }

        foreach (var onEnable in onEnablesUnconditional)
        {
            RegisterMethod(onEnable, onEnable.OnEnableUnconditional, CallbackUpdate.EnableUnconditional);
        }

        foreach (var onPreUpdate in onPreUpdatesUnconditional)
        {
            RegisterMethod(onPreUpdate, onPreUpdate.PreUpdateUnconditional, CallbackUpdate.PreUpdateUnconditional);
        }

        foreach (var onUpdate in onUpdatesUnconditional)
        {
            RegisterMethod(onUpdate, onUpdate.UpdateUnconditional, CallbackUpdate.UpdateUnconditional);
        }

        foreach (var onFixedUpdate in onFixedUpdatesUnconditional)
        {
            RegisterMethod(onFixedUpdate, onFixedUpdate.FixedUpdateUnconditional,
                CallbackUpdate.FixedUpdateUnconditional);
        }

        foreach (var onGui in onGuisUnconditional)
        {
            RegisterMethod(onGui, onGui.OnGUIUnconditional, CallbackUpdate.GUIUnconditional);
        }

        foreach (var onPreUpdateActual in onPreUpdatesActual)
        {
            RegisterMethod(onPreUpdateActual, onPreUpdateActual.PreUpdateActual, CallbackUpdate.PreUpdateActual);
        }

        foreach (var onFixedUpdateActual in onFixedUpdatesActual)
        {
            RegisterMethod(onFixedUpdateActual, onFixedUpdateActual.FixedUpdateActual,
                CallbackUpdate.FixedUpdateActual);
        }

        foreach (var onStartActual in onStartsActual)
        {
            RegisterMethod(onStartActual, onStartActual.StartActual, CallbackUpdate.StartActual);
        }

        foreach (var onUpdateActual in onUpdatesActual)
        {
            RegisterMethod(onUpdateActual, onUpdateActual.UpdateActual, CallbackUpdate.UpdateActual);
        }

        foreach (var onLateUpdateUnconditional in onLateUpdatesUnconditional)
        {
            RegisterMethod(onLateUpdateUnconditional, onLateUpdateUnconditional.OnLateUpdateUnconditional,
                CallbackUpdate.LateUpdateUnconditional);
        }

        foreach (var onLastUpdateUnconditional in onLastUpdatesUnconditional)
        {
            RegisterMethod(onLastUpdateUnconditional, onLastUpdateUnconditional.OnLastUpdateUnconditional,
                CallbackUpdate.LastUpdateUnconditional);
        }

        foreach (var onLastUpdateActual in onLastUpdatesActual)
        {
            RegisterMethod(onLastUpdateActual, onLastUpdateActual.OnLastUpdateActual, CallbackUpdate.LastUpdateActual);
        }

        foreach (var endOfFrameActual in onEndOfFrameActual)
        {
            RegisterMethod(endOfFrameActual, endOfFrameActual.OnEndOfFrame, CallbackUpdate.EndOfFrameActual);
        }

        // input system events init
        foreach (var onInputUpdateActual in onInputUpdatesActual)
        {
            RegisterMethod(onInputUpdateActual, onInputUpdateActual.InputUpdateActual,
                CallbackInputUpdate.InputUpdateActual);
        }

        foreach (var onInputUpdateUnconditional in onInputUpdatesUnconditional)
        {
            RegisterMethod(onInputUpdateUnconditional, onInputUpdateUnconditional.InputUpdateUnconditional,
                CallbackInputUpdate.InputUpdateUnconditional);
        }

        InputSystemEventsInit();
    }

    private void RegisterMethod(object processingCallback, Action callback, CallbackUpdate update)
    {
        var callbackList = update switch
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
            CallbackUpdate.LateUpdateUnconditional => _lateUpdatesUnconditional,
            CallbackUpdate.LateUpdateActual => _lateUpdatesActual,
            CallbackUpdate.EndOfFrameActual => _endOfFramesActual,
            _ => throw new ArgumentOutOfRangeException(nameof(update), update, null)
        };

        if (processingCallback is IUnityEventPriority unityEventPriority)
        {
            var priorityKey = new Either<CallbackUpdate, CallbackInputUpdate>(update);
            if (unityEventPriority.Priorities.TryGetValue(priorityKey, out var priority))
            {
                unityEventPriority.Priorities.Remove(new(CallbackUpdate.UpdateUnconditional));
                callbackList.Add(callback, (int)priority);
                return;
            }
        }

        callbackList.Add(callback, (int)CallbackPriority.Default);
    }

    private void RegisterMethod(object processingCallback, IUpdateEvents.InputUpdateCall callback,
        CallbackInputUpdate update)
    {
        var callbackList = update switch
        {
            CallbackInputUpdate.InputUpdateActual => _inputUpdatesActual,
            CallbackInputUpdate.InputUpdateUnconditional => _inputUpdatesUnconditional,
            _ => throw new ArgumentOutOfRangeException(nameof(update), update, null)
        };

        if (processingCallback is IUnityEventPriority unityEventPriority)
        {
            var priorityKey = new Either<CallbackUpdate, CallbackInputUpdate>(update);
            if (unityEventPriority.Priorities.TryGetValue(priorityKey, out var priority))
            {
                unityEventPriority.Priorities.Remove(new(CallbackUpdate.UpdateUnconditional));
                callbackList.Add(callback, (int)priority);
                return;
            }
        }

        callbackList.Add(callback, (int)CallbackPriority.Default);
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

    private readonly PriorityList<Action> _endOfFramesActual = new();

    private bool _updated;
    private bool _calledPreUpdate;

    private readonly IMonoBehaviourController _monoBehaviourController;

    public void InvokeLastUpdate()
    {
#if TRACE
        StaticLogger.Trace($"InvokeLastUpdate, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)}, " +
                           $"paused: {_monoBehaviourController.PausedExecution}");
#endif

        for (var i = 0; i < _lastUpdatesUnconditional.Count; i++)
        {
            _lastUpdatesUnconditional[i]();
        }

        for (var i = 0; i < _lastUpdatesActual.Count; i++)
        {
            var lastUpdate = _lastUpdatesActual[i];
            if (_monoBehaviourController.PausedExecution || _monoBehaviourController.PausedUpdate) continue;
            lastUpdate();
        }
    }

    // calls awake before any other script
    public void InvokeAwake()
    {
#if TRACE
        StaticLogger.Trace($"InvokeAwake, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)}, " +
                           $"paused: {_monoBehaviourController.PausedExecution}");
#endif

        for (var i = 0; i < _awakesUnconditional.Count; i++)
        {
            _awakesUnconditional[i]();
        }

        for (var i = 0; i < _awakesActual.Count; i++)
        {
            var awake = _awakesActual[i];
            if (!_monoBehaviourController.PausedExecution)
                awake();
        }
    }

    // calls onEnable before any other script
    public void InvokeOnEnable()
    {
#if TRACE
        StaticLogger.Trace($"InvokeOnEnable, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)}, " +
                           $"paused: {_monoBehaviourController.PausedExecution}");
#endif

        for (var i = 0; i < _enablesUnconditional.Count; i++)
        {
            _enablesUnconditional[i]();
        }

        for (var i = 0; i < _enablesActual.Count; i++)
        {
            var enable = _enablesActual[i];
            if (!_monoBehaviourController.PausedExecution)
                enable();
        }
    }

    // calls start before any other script
    public void InvokeStart()
    {
#if TRACE
        StaticLogger.Trace($"InvokeStart, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)}, " +
                           $"paused: {_monoBehaviourController.PausedExecution}");
#endif

        for (var i = 0; i < _startsUnconditional.Count; i++)
        {
            _startsUnconditional[i]();
        }

        for (var i = 0; i < _startsActual.Count; i++)
        {
            var start = _startsActual[i];
            if (!_monoBehaviourController.PausedExecution)
                start();
        }
    }

    public void InvokeUpdate()
    {
        if (_updated) return;
        _updated = true;

#if TRACE
        StaticLogger.Trace($"InvokeUpdate, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)}, " +
                           $"paused: {_monoBehaviourController.PausedExecution || _monoBehaviourController.PausedUpdate}");
#endif

        if (!_calledPreUpdate)
        {
            _calledPreUpdate = true;
            InvokeCallOnPreUpdate();
        }

        _endOfFrameUpdated = false;

        for (var i = 0; i < _updatesUnconditional.Count; i++)
        {
            _updatesUnconditional[i]();
        }

        for (var i = 0; i < _updatesActual.Count; i++)
        {
            var update = _updatesActual[i];
            if (_monoBehaviourController.PausedExecution ||
                _monoBehaviourController.PausedUpdate) continue;
            update();
        }
    }

    // right now I don't call this update before other scripts so I don't need to check if it was already called
    public void InvokeLateUpdate()
    {
#if TRACE
        StaticLogger.Trace($"InvokeLateUpdate, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)}, " +
                           $"paused: {_monoBehaviourController.PausedExecution}");
#endif

        _updated = false;
        _calledPreUpdate = false;

        for (var i = 0; i < _lateUpdatesUnconditional.Count; i++)
        {
            _lateUpdatesUnconditional[i]();
        }

        for (var i = 0; i < _lateUpdatesActual.Count; i++)
        {
            var lateUpdate = _lateUpdatesActual[i];
            if (_monoBehaviourController.PausedExecution ||
                _monoBehaviourController.PausedUpdate) continue;
            lateUpdate();
        }
    }

    private float _prevFixedTime = -1;

    public void InvokeFixedUpdate()
    {
#if !UNIT_TESTS
        var fixedTime = _patchReverseInvoker.Invoke(() => Time.fixedTime);
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_prevFixedTime == fixedTime) return;
        _prevFixedTime = fixedTime;
#endif

#if TRACE
        StaticLogger.Trace(
            $"InvokeFixedUpdate, time: {fixedTime}, paused: {_monoBehaviourController.PausedExecution}");
#endif

        InvokeCallOnPreUpdate();

        for (var i = 0; i < _fixedUpdatesUnconditional.Count; i++)
        {
            _fixedUpdatesUnconditional[i]();
        }

        for (var i = 0; i < _fixedUpdatesActual.Count; i++)
        {
            var fixedUpdate = _fixedUpdatesActual[i];
            if (!_monoBehaviourController.PausedExecution)
                fixedUpdate();
        }
    }

    public void InvokeOnGUI()
    {
// #if TRACE
//         StaticLogger.Trace($"InvokeOnGUI, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)}, " +
//                            $"paused: {_monoBehaviourController.PausedExecution}");
// #endif

        // currently, this doesn't get called before other scripts
        for (var i = 0; i < _guisUnconditional.Count; i++)
        {
            _guisUnconditional[i]();
        }

        for (var i = 0; i < _guisActual.Count; i++)
        {
            var gui = _guisActual[i];
            if (!_monoBehaviourController.PausedExecution)
                gui();
        }
    }

    private void InvokeCallOnPreUpdate()
    {
#if TRACE
        StaticLogger.Trace($"InvokeCallOnPreUpdate, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)}, " +
                           $"paused: {_monoBehaviourController.PausedExecution}");
#endif

        for (var i = 0; i < _preUpdatesUnconditional.Count; i++)
        {
            _preUpdatesUnconditional[i]();
        }

        for (var i = 0; i < _preUpdatesActual.Count; i++)
        {
            var preUpdate = _preUpdatesActual[i];
            if (!_monoBehaviourController.PausedExecution)
                preUpdate();
        }
    }

    private bool _endOfFrameUpdated;

    public void InvokeEndOfFrame()
    {
        if (_endOfFrameUpdated) return;
        _endOfFrameUpdated = true;

#if TRACE
        StaticLogger.Trace($"InvokeEndOfFrame, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)}, " +
                           $"paused: {_monoBehaviourController.PausedExecution}");
#endif

        // for (var i = 0; i < _endOfFramesUnconditional.Count; i++)
        // {
        //     _endOfFramesUnconditional[i]();
        // }

        for (var i = 0; i < _endOfFramesActual.Count; i++)
        {
            var endOfFrame = _endOfFramesActual[i];
            if (_monoBehaviourController.PausedExecution ||
                _monoBehaviourController.PausedUpdate) continue;
            endOfFrame();
        }
    }
}