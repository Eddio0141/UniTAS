using System;

namespace UniTAS.Patcher.Shared;

/// <summary>
/// Event calls for MonoBehaviour event methods
/// Unconditional will be called before Actual
/// Unconditional events are called even if MonoBehaviour is paused
/// Actual events are called only if MonoBehaviour is not paused
/// </summary>
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
    public static event Action OnUpdateActual;
    public static event Action OnFixedUpdateUnconditional;
    public static event Action OnFixedUpdateActual;
    public static event Action OnGUIUnconditional;
    public static event Action OnGUIActual;

    private static bool _updatedUnconditional;
    private static bool _calledFixedUpdateUnconditional;
    private static bool _calledPreUpdateUnconditional;

    private static bool _updatedActual;
    private static bool _calledFixedUpdateActual;
    private static bool _calledPreUpdateActual;

    static MonoBehaviourEvents()
    {
        // TODO ew
        UpdateInvokeOffset.Init();
    }

    // calls awake before any other script
    public static void InvokeAwakeUnconditional()
    {
        OnAwakeUnconditional?.Invoke();
    }

    public static void InvokeAwakeActual()
    {
        OnAwakeActual?.Invoke();
    }

    // calls onEnable before any other script
    public static void InvokeOnEnableUnconditional()
    {
        OnEnableUnconditional?.Invoke();
    }

    public static void InvokeOnEnableActual()
    {
        OnEnableActual?.Invoke();
    }

    // calls start before any other script
    public static void InvokeStartUnconditional()
    {
        OnStartUnconditional?.Invoke();
    }

    public static void InvokeStartActual()
    {
        OnStartActual?.Invoke();
    }

    public static void InvokeUpdateUnconditional()
    {
        if (_updatedUnconditional) return;
        _updatedUnconditional = true;

        _calledFixedUpdateUnconditional = false;

        InvokeCallOnPreUpdateUnconditional();

        OnUpdateUnconditional?.Invoke();
    }

    public static void InvokeUpdateActual()
    {
        if (_updatedActual) return;
        _updatedActual = true;

        _calledFixedUpdateActual = false;

        InvokeCallOnPreUpdateActual();

        OnUpdateActual?.Invoke();
    }

    // right now I don't call this update before other scripts so I don't need to check if it was already called
    public static void InvokeLateUpdateUnconditional()
    {
        _updatedUnconditional = false;
        _calledPreUpdateUnconditional = false;
    }

    public static void InvokeLateUpdateActual()
    {
        _updatedActual = false;
        _calledPreUpdateActual = false;
    }

    public static void InvokeFixedUpdateUnconditional()
    {
        if (_calledFixedUpdateUnconditional) return;
        _calledFixedUpdateUnconditional = true;

        InvokeCallOnPreUpdateUnconditional();

        OnFixedUpdateUnconditional?.Invoke();
    }

    public static void InvokeFixedUpdateActual()
    {
        if (_calledFixedUpdateActual) return;
        _calledFixedUpdateActual = true;

        InvokeCallOnPreUpdateActual();

        OnFixedUpdateActual?.Invoke();
    }

    public static void InvokeOnGUIUnconditional()
    {
        // currently, this doesn't get called before other scripts
        OnGUIUnconditional?.Invoke();
    }

    public static void InvokeOnGUIActual()
    {
        // currently, this doesn't get called before other scripts
        OnGUIActual?.Invoke();
    }

    private static void InvokeCallOnPreUpdateUnconditional()
    {
        if (_calledPreUpdateUnconditional) return;
        _calledPreUpdateUnconditional = true;

        OnPreUpdateUnconditional?.Invoke();
    }

    private static void InvokeCallOnPreUpdateActual()
    {
        if (_calledPreUpdateActual) return;
        _calledPreUpdateActual = true;

        OnPreUpdateActual?.Invoke();
    }
}