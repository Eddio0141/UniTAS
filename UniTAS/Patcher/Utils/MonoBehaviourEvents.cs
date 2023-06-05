using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.Invoker;

namespace UniTAS.Patcher.Utils;

/// <summary>
/// Event calls for MonoBehaviour event methods
/// Unconditional will be called before Actual
/// Unconditional events are called even if MonoBehaviour is paused
/// Actual events are called only if MonoBehaviour is not paused
/// </summary>
[InvokeOnUnityInit]
public static class MonoBehaviourEvents
{
    public static event Action OnAwakeUnconditional;
    public static event Action OnAwakeActual;
    public static event Action OnStartUnconditional;
    public static event Action OnStartActual;
    public static event Action OnEnableUnconditional;
    public static event Action OnEnableActual;
    public static event Action OnPreUpdateUnconditional;
    public static event Action OnPreUpdateActual;
    public static event Action OnUpdateUnconditional;

    public static event Action OnUpdateActual
    {
        add => UpdatesActual.Add(value);
        remove => UpdatesActual.Remove(value);
    }

    public static event Action OnFixedUpdateUnconditional;
    public static event Action OnFixedUpdateActual;
    public static event Action OnGUIUnconditional;
    public static event Action OnGUIActual;

    private static readonly List<Action> UpdatesActual = new();

    private static bool _updated;
    private static bool _calledFixedUpdate;
    private static bool _calledPreUpdate;

    // calls awake before any other script
    public static void InvokeAwake()
    {
        OnAwakeUnconditional?.Invoke();
        if (!MonoBehaviourController.PausedExecution)
            OnAwakeActual?.Invoke();
    }

    // calls onEnable before any other script
    public static void InvokeOnEnable()
    {
        OnEnableUnconditional?.Invoke();
        if (!MonoBehaviourController.PausedExecution)
            OnEnableActual?.Invoke();
    }

    // calls start before any other script
    public static void InvokeStart()
    {
        OnStartUnconditional?.Invoke();
        if (!MonoBehaviourController.PausedExecution)
            OnStartActual?.Invoke();
    }

    public static void InvokeUpdate()
    {
        if (_updated) return;
        _updated = true;

        _calledFixedUpdate = false;

        if (!_calledPreUpdate)
        {
            _calledPreUpdate = true;
            InvokeCallOnPreUpdate();
        }

        OnUpdateUnconditional?.Invoke();

        foreach (var update in UpdatesActual)
        {
            if (MonoBehaviourController.PausedExecution) continue;
            update();
        }
    }

    // right now I don't call this update before other scripts so I don't need to check if it was already called
    public static void InvokeLateUpdate()
    {
        _updated = false;
        _calledPreUpdate = false;
    }

    public static void InvokeFixedUpdate()
    {
        if (_calledFixedUpdate) return;
        _calledFixedUpdate = true;

        InvokeCallOnPreUpdate();

        OnFixedUpdateUnconditional?.Invoke();

        if (!MonoBehaviourController.PausedExecution)
            OnFixedUpdateActual?.Invoke();
    }

    public static void InvokeOnGUI()
    {
        // currently, this doesn't get called before other scripts
        OnGUIUnconditional?.Invoke();
        if (!MonoBehaviourController.PausedExecution)
            OnGUIActual?.Invoke();
    }

    private static void InvokeCallOnPreUpdate()
    {
        OnPreUpdateUnconditional?.Invoke();
        if (!MonoBehaviourController.PausedExecution)
            OnPreUpdateActual?.Invoke();
    }
}