using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents;
using UniTAS.Patcher.ManualServices;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Models.Utils;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.UnityEvents;

/// <summary>
/// Event calls for MonoBehaviour event methods
/// Unconditional will be called before Actual
/// Unconditional events are called even if MonoBehaviour is paused
/// Actual events are called only if MonoBehaviour is not paused
/// </summary>
[Singleton(timing: RegisterTiming.Entry)]
public class UnityEvents(IMonoBehaviourController monoBehaviourController, IPatchReverseInvoker patchReverseInvoker)
    : IUpdateEvents, IMonoBehEventInvoker
{
    public void RegisterMethod(object processingCallback, Action callback, CallbackUpdate update)
    {
        var callbackList = update switch
        {
            CallbackUpdate.AwakeActual => _awakesActual,
            CallbackUpdate.AwakeUnconditional => _awakesUnconditional,
            CallbackUpdate.StartActual => _startsActual,
            CallbackUpdate.StartUnconditional => _startsUnconditional,
            CallbackUpdate.EnableActual => _enablesActual,
            CallbackUpdate.EnableUnconditional => _enablesUnconditional,
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
            if (unityEventPriority.Priorities.TryGetValue(update, out var priority))
            {
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

    public event Action OnEndOfFrameActual
    {
        add => _endOfFramesActual.Add(value, (int)CallbackPriority.Default);
        remove => _endOfFramesActual.Remove(value);
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
    private readonly UnityEventList<Action> _awakesUnconditional = new();
    private readonly UnityEventList<Action> _awakesActual = new();

    private readonly UnityEventList<Action> _startsUnconditional = new();
    private readonly UnityEventList<Action> _startsActual = new();

    private readonly UnityEventList<Action> _enablesUnconditional = new();
    private readonly UnityEventList<Action> _enablesActual = new();

    private readonly UnityEventList<Action> _updatesUnconditional = new();
    private readonly UnityEventList<Action> _updatesActual = new();

    private readonly UnityEventList<Action> _fixedUpdatesUnconditional = new();
    private readonly UnityEventList<Action> _fixedUpdatesActual = new();

    private readonly UnityEventList<Action> _guisUnconditional = new();
    private readonly UnityEventList<Action> _guisActual = new();

    private readonly UnityEventList<Action> _lateUpdatesUnconditional = new();
    private readonly UnityEventList<Action> _lateUpdatesActual = new();

    private readonly UnityEventList<Action> _lastUpdatesUnconditional = new();
    private readonly UnityEventList<Action> _lastUpdatesActual = new();

    private readonly UnityEventList<Action> _endOfFramesActual = new();

    private bool _updated;
    private bool _calledLastUpdate = true; // true initially to stop printing error, Update will run first anyways

    public void InvokeLastUpdate()
    {
#if TRACE
        StaticLogger.Trace(
            $"InvokeLastUpdate, time: {patchReverseInvoker.Invoke(() => Time.frameCount)} ({Time.frameCount}), " +
            $"paused: {monoBehaviourController.PausedExecution}");
#endif

        if (_calledLastUpdate)
        {
            StaticLogger.LogError($"last update was called multiple times in one frame, {Environment.StackTrace}");
            return;
        }

        _calledLastUpdate = true;

        foreach (var action in _lastUpdatesUnconditional)
        {
            var bench = Bench.Measure();
            action();
            bench.Dispose();
        }

        foreach (var action in _lastUpdatesActual)
        {
            var bench = Bench.Measure();
            if (!monoBehaviourController.PausedExecution)
                action();
            bench.Dispose();
        }
    }

    // calls awake before any other script
    public void InvokeAwake()
    {
#if TRACE
        StaticLogger.Trace(
            $"InvokeAwake, time: {patchReverseInvoker.Invoke(() => Time.frameCount)} ({Time.frameCount}), " +
            $"paused: {monoBehaviourController.PausedExecution}");
#endif

        foreach (var action in _awakesUnconditional)
        {
            action();
        }

        foreach (var action in _awakesActual)
        {
            if (!monoBehaviourController.PausedExecution)
                action();
        }
    }

    // calls onEnable before any other script
    public void InvokeOnEnable()
    {
#if TRACE
        StaticLogger.Trace(
            $"InvokeOnEnable, time: {patchReverseInvoker.Invoke(() => Time.frameCount)} ({Time.frameCount}), " +
            $"paused: {monoBehaviourController.PausedExecution}");
#endif

        foreach (var action in _enablesUnconditional)
        {
            action();
        }

        foreach (var action in _enablesActual)
        {
            if (!monoBehaviourController.PausedExecution)
                action();
        }
    }

    // calls start before any other script
    public void InvokeStart()
    {
#if TRACE
        StaticLogger.Trace(
            $"InvokeStart, time: {patchReverseInvoker.Invoke(() => Time.frameCount)} ({Time.frameCount}), " +
            $"paused: {monoBehaviourController.PausedExecution}");
#endif

        foreach (var action in _startsUnconditional)
        {
            action();
        }

        foreach (var action in _startsActual)
        {
            if (!monoBehaviourController.PausedExecution)
                action();
        }
    }

    public void InvokeUpdate()
    {
        if (_updated) return;
        _updated = true;

#if TRACE
        StaticLogger.Trace(
            $"InvokeUpdate, time: {patchReverseInvoker.Invoke(() => Time.frameCount)} ({Time.frameCount}), " +
            $"paused: {monoBehaviourController.PausedExecution}");
#endif

        if (!_calledLastUpdate)
        {
            StaticLogger.LogError("LastUpdate was not called in the previous frame, something is wrong");
        }

        _endOfFrameUpdated = false;
        _calledLastUpdate = false;

        foreach (var action in _updatesUnconditional)
        {
            var bench = Bench.Measure();
            action();
            bench.Dispose();
        }

        foreach (var action in _updatesActual)
        {
            var bench = Bench.Measure();
            if (!monoBehaviourController.PausedExecution)
                action();
            bench.Dispose();
        }
    }

    // right now I don't call this update before other scripts so I don't need to check if it was already called
    public void InvokeLateUpdate()
    {
#if TRACE
        StaticLogger.Trace(
            $"InvokeLateUpdate, time: {patchReverseInvoker.Invoke(() => Time.frameCount)} ({Time.frameCount}), " +
            $"paused: {monoBehaviourController.PausedExecution}");
#endif

        _updated = false;

        foreach (var action in _lateUpdatesUnconditional)
        {
            var bench = Bench.Measure();
            action();
            bench.Dispose();
        }

        foreach (var action in _lateUpdatesActual)
        {
            var bench = Bench.Measure();
            if (!monoBehaviourController.PausedExecution)
                action();
            bench.Dispose();
        }
    }

    private float _prevFixedTime = -1;

    public void InvokeFixedUpdate()
    {
#if !UNIT_TESTS
        var fixedTime = patchReverseInvoker.Invoke(() => Time.fixedTime);
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_prevFixedTime == fixedTime) return;
        _prevFixedTime = fixedTime;
#endif

#if TRACE
        StaticLogger.Trace(
            $"InvokeFixedUpdate, time: {fixedTime}, paused: {monoBehaviourController.PausedExecution}");
#endif

        foreach (var action in _fixedUpdatesUnconditional)
        {
            var bench = Bench.Measure();
            action();
            bench.Dispose();
        }

        foreach (var action in _fixedUpdatesActual)
        {
            var bench = Bench.Measure();
            if (!monoBehaviourController.PausedExecution)
                action();
            bench.Dispose();
        }
    }

    public void InvokeOnGUI()
    {
// #if TRACE
//         StaticLogger.Trace($"InvokeOnGUI, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)}, " +
//                            $"paused: {_monoBehaviourController.PausedExecution}");
// #endif

        // currently, this doesn't get called before other scripts
        foreach (var action in _guisUnconditional)
        {
            var bench = Bench.Measure();
            action();
            bench.Dispose();
        }

        foreach (var action in _guisActual)
        {
            var bench = Bench.Measure();
            if (!monoBehaviourController.PausedExecution)
                action();
            bench.Dispose();
        }
    }

    private bool _endOfFrameUpdated;

    public void InvokeEndOfFrame()
    {
        if (_endOfFrameUpdated) return;
        _endOfFrameUpdated = true;

#if TRACE
        StaticLogger.Trace(
            $"InvokeEndOfFrame, time: {patchReverseInvoker.Invoke(() => Time.frameCount)} ({Time.frameCount}), " +
            $"paused: {monoBehaviourController.PausedExecution}");
#endif

        // _endOfFramesUnconditional

        foreach (var action in _endOfFramesActual)
        {
            var bench = Bench.Measure();
            if (!monoBehaviourController.PausedExecution)
                action();
            bench.Dispose();
        }
    }
}