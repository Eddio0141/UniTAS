using System;
using UniTAS.Patcher.External;
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
public class UnityEvents : IUpdateEvents, IMonoBehEventInvoker
{
    public void RegisterMethod(object processingCallback, Action callback, CallbackUpdate update)
    {
        var (callbackList, actual) = update switch
        {
            CallbackUpdate.AwakeUnconditional => (_awakes, false),
            CallbackUpdate.AwakeActual => (_awakes, true),
            CallbackUpdate.StartUnconditional => (_starts, false),
            CallbackUpdate.StartActual => (_starts, true),
            CallbackUpdate.EnableUnconditional => (_enables, false),
            CallbackUpdate.EnableActual => (_enables, true),
            CallbackUpdate.UpdateUnconditional => (_updates, false),
            CallbackUpdate.UpdateActual => (_updates, true),
            CallbackUpdate.FixedUpdateUnconditional => (_fixedUpdates, false),
            CallbackUpdate.FixedUpdateActual => (_fixedUpdates, true),
            CallbackUpdate.GUIUnconditional => (_guis, false),
            CallbackUpdate.GUIActual => (_guis, true),
            CallbackUpdate.LateUpdateUnconditional => (_lateUpdates, false),
            CallbackUpdate.LateUpdateActual => (_lateUpdates, true),
            CallbackUpdate.EndOfFrameActual => (_endOfFrames, false),
            CallbackUpdate.LastUpdateUnconditional => (_lastUpdates, false),
            CallbackUpdate.LastUpdateActual => (_lastUpdates, true),
            _ => throw new ArgumentOutOfRangeException(nameof(update), update, null)
        };

        if (processingCallback is IUnityEventPriority unityEventPriority)
        {
            if (unityEventPriority.Priorities.TryGetValue(update, out var priority))
            {
                callbackList.Add(new UnityEvent(callback, actual), (int)priority);
                return;
            }
        }

        callbackList.Add(new UnityEvent(callback, actual), (int)CallbackPriority.Default);
    }

    public void AddPriorityCallback(CallbackUpdate callbackUpdate, Action callback, CallbackPriority priority)
    {
        var (callbackList, actual) = CallbackUpdateToList(callbackUpdate);
        callbackList.Add(new UnityEvent(callback, actual), (int)priority);
    }

    private (UnityEventList<UnityEvent>, bool) CallbackUpdateToList(CallbackUpdate callbackUpdate)
    {
        return callbackUpdate switch
        {
            CallbackUpdate.AwakeUnconditional => (_awakes, false),
            CallbackUpdate.AwakeActual => (_awakes, true),
            CallbackUpdate.StartUnconditional => (_starts, false),
            CallbackUpdate.StartActual => (_starts, true),
            CallbackUpdate.EnableUnconditional => (_enables, false),
            CallbackUpdate.EnableActual => (_enables, true),
            CallbackUpdate.UpdateUnconditional => (_updates, false),
            CallbackUpdate.UpdateActual => (_updates, true),
            CallbackUpdate.FixedUpdateUnconditional => (_fixedUpdates, false),
            CallbackUpdate.FixedUpdateActual => (_fixedUpdates, true),
            CallbackUpdate.GUIUnconditional => (_guis, false),
            CallbackUpdate.GUIActual => (_guis, true),
            CallbackUpdate.LateUpdateUnconditional => (_lateUpdates, false),
            CallbackUpdate.LateUpdateActual => (_lateUpdates, true),
            CallbackUpdate.EndOfFrameActual => (_endOfFrames, false),
            CallbackUpdate.LastUpdateUnconditional => (_lastUpdates, false),
            CallbackUpdate.LastUpdateActual => (_lastUpdates, true),
            _ => throw new ArgumentOutOfRangeException(nameof(callbackUpdate), callbackUpdate, null)
        };
    }

    public event Action OnAwakeUnconditional
    {
        add => _awakes.Add(new UnityEvent(value, false), (int)CallbackPriority.Default);
        remove => _awakes.Remove(new UnityEvent(value, false));
    }

    public event Action OnAwakeActual
    {
        add => _awakes.Add(new UnityEvent(value, true), (int)CallbackPriority.Default);
        remove => _awakes.Remove(new UnityEvent(value, true));
    }

    public event Action OnStartUnconditional
    {
        add => _starts.Add(new UnityEvent(value, false), (int)CallbackPriority.Default);
        remove => _starts.Remove(new UnityEvent(value, false));
    }

    public event Action OnStartActual
    {
        add => _starts.Add(new UnityEvent(value, true), (int)CallbackPriority.Default);
        remove => _starts.Remove(new UnityEvent(value, true));
    }

    public event Action OnEnableUnconditional
    {
        add => _enables.Add(new UnityEvent(value, false), (int)CallbackPriority.Default);
        remove => _enables.Remove(new UnityEvent(value, false));
    }

    public event Action OnEnableActual
    {
        add => _enables.Add(new UnityEvent(value, true), (int)CallbackPriority.Default);
        remove => _enables.Remove(new UnityEvent(value, true));
    }

    public event Action OnUpdateUnconditional
    {
        add => _updates.Add(new UnityEvent(value, false), (int)CallbackPriority.Default);
        remove => _updates.Remove(new UnityEvent(value, false));
    }

    public event Action OnUpdateActual
    {
        add => _updates.Add(new UnityEvent(value, true), (int)CallbackPriority.Default);
        remove => _updates.Remove(new UnityEvent(value, true));
    }

    public event Action OnFixedUpdateUnconditional
    {
        add => _fixedUpdates.Add(new UnityEvent(value, false), (int)CallbackPriority.Default);
        remove => _fixedUpdates.Remove(new UnityEvent(value, false));
    }

    public event Action OnFixedUpdateActual
    {
        add => _fixedUpdates.Add(new UnityEvent(value, true), (int)CallbackPriority.Default);
        remove => _fixedUpdates.Remove(new UnityEvent(value, true));
    }

    public event Action OnGUIUnconditional
    {
        add => _guis.Add(new UnityEvent(value, false), (int)CallbackPriority.Default);
        remove => _guis.Remove(new UnityEvent(value, false));
    }

    public event Action OnGUIActual
    {
        add => _guis.Add(new UnityEvent(value, true), (int)CallbackPriority.Default);
        remove => _guis.Remove(new UnityEvent(value, true));
    }

    public event Action OnLateUpdateUnconditional
    {
        add => _lateUpdates.Add(new UnityEvent(value, false), (int)CallbackPriority.Default);
        remove => _lateUpdates.Remove(new UnityEvent(value, false));
    }

    public event Action OnLateUpdateActual
    {
        add => _lateUpdates.Add(new UnityEvent(value, true), (int)CallbackPriority.Default);
        remove => _lateUpdates.Remove(new UnityEvent(value, true));
    }

    public event Action OnEndOfFrameActual
    {
        add => _endOfFrames.Add(new UnityEvent(value, true), (int)CallbackPriority.Default);
        remove => _endOfFrames.Remove(new UnityEvent(value, true));
    }

    public event Action OnLastUpdateUnconditional
    {
        add => _lastUpdates.Add(new UnityEvent(value, false), (int)CallbackPriority.Default);
        remove => _lastUpdates.Remove(new UnityEvent(value, false));
    }

    public event Action OnLastUpdateActual
    {
        add => _lastUpdates.Add(new UnityEvent(value, true), (int)CallbackPriority.Default);
        remove => _lastUpdates.Remove(new UnityEvent(value, true));
    }

    private readonly struct UnityEvent(Action callback, bool actual)
    {
        public readonly Action Callback = callback;
        public readonly bool Actual = actual;
    }

    private readonly UnityEventList<UnityEvent> _awakes = new();
    private readonly UnityEventList<UnityEvent> _starts = new();
    private readonly UnityEventList<UnityEvent> _enables = new();
    private readonly UnityEventList<UnityEvent> _updates = new();
    private readonly UnityEventList<UnityEvent> _fixedUpdates = new();
    private readonly UnityEventList<UnityEvent> _guis = new();
    private readonly UnityEventList<UnityEvent> _lateUpdates = new();
    private readonly UnityEventList<UnityEvent> _lastUpdates = new();
    private readonly UnityEventList<UnityEvent> _endOfFrames = new();

    private bool _updated;
    private bool _calledLastUpdate = true; // true initially to stop printing error, Update will run first anyways

    private static void InvokeLastUpdateStatic() => _unityEvent.InvokeLastUpdate();

    private void InvokeLastUpdate()
    {
#if TRACE
        StaticLogger.Trace(
            $"InvokeLastUpdate, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)} ({Time.frameCount}), " +
            $"paused: {_monoBehaviourController.PausedExecution}");
#endif

        if (_calledLastUpdate)
        {
            StaticLogger.LogError($"last update was called multiple times in one frame, {Environment.StackTrace}");
            return;
        }

        _calledLastUpdate = true;

        HandleCallbacks(_lastUpdates);
    }

    // calls awake before any other script
    public void InvokeAwake()
    {
#if TRACE
        StaticLogger.Trace(
            $"InvokeAwake, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)} ({Time.frameCount}), " +
            $"paused: {_monoBehaviourController.PausedExecution}");
#endif

        HandleCallbacks(_awakes);
    }

    // calls onEnable before any other script
    public void InvokeOnEnable()
    {
#if TRACE
        StaticLogger.Trace(
            $"InvokeOnEnable, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)} ({Time.frameCount}), " +
            $"paused: {_monoBehaviourController.PausedExecution}");
#endif

        HandleCallbacks(_enables);
    }

    // calls start before any other script
    public void InvokeStart()
    {
#if TRACE
        StaticLogger.Trace(
            $"InvokeStart, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)} ({Time.frameCount}), " +
            $"paused: {_monoBehaviourController.PausedExecution}");
#endif

        HandleCallbacks(_starts);
    }

    public void InvokeUpdate()
    {
        if (_updated) return;
        _updated = true;

#if TRACE
        StaticLogger.Trace(
            $"InvokeUpdate, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)} ({Time.frameCount}), " +
            $"paused: {_monoBehaviourController.PausedExecution}");
#endif

        if (!_calledLastUpdate)
        {
            StaticLogger.LogError("LastUpdate was not called in the previous frame, something is wrong");
        }

        _endOfFrameUpdated = false;
        _calledLastUpdate = false;

        HandleCallbacks(_updates);
    }

    // right now I don't call this update before other scripts so I don't need to check if it was already called
    public void InvokeLateUpdate()
    {
#if TRACE
        StaticLogger.Trace(
            $"InvokeLateUpdate, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)} ({Time.frameCount}), " +
            $"paused: {_monoBehaviourController.PausedExecution}");
#endif

        _updated = false;

        HandleCallbacks(_lateUpdates);
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

        HandleCallbacks(_fixedUpdates);
    }

    public void InvokeOnGUI()
    {
        // currently, this doesn't get called before other scripts
        HandleCallbacks(_guis);
    }

    private bool _endOfFrameUpdated;
    private readonly IMonoBehaviourController _monoBehaviourController;
    private readonly IPatchReverseInvoker _patchReverseInvoker;
    private static UnityEvents _unityEvent;

    public UnityEvents(IMonoBehaviourController monoBehaviourController, IPatchReverseInvoker patchReverseInvoker)
    {
        _monoBehaviourController = monoBehaviourController;
        _patchReverseInvoker = patchReverseInvoker;
        _unityEvent = this;
        UniTasRs.last_update_set_callback(InvokeLastUpdateStatic);
    }

    public void InvokeEndOfFrame()
    {
        if (_endOfFrameUpdated) return;
        _endOfFrameUpdated = true;

#if TRACE
        StaticLogger.Trace(
            $"InvokeEndOfFrame, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)} ({Time.frameCount}), " +
            $"paused: {_monoBehaviourController.PausedExecution}");
#endif

        // _endOfFramesUnconditional

        HandleCallbacks(_endOfFrames);
    }

    private void HandleCallbacks(UnityEventList<UnityEvent> events)
    {
        foreach (var action in events)
        {
            if (_monoBehaviourController.PausedExecution && action.Actual) continue;
            var bench = Bench.Measure();
            action.Callback();
            bench.Dispose();
        }
    }
}