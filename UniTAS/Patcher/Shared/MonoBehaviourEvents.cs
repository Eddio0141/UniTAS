using System;

namespace UniTAS.Patcher.Shared;

public static class MonoBehaviourEvents
{
    public static event Action OnAwake;
    public static event Action OnStart;
    public static event Action OnEnable;
    public static event Action OnPreUpdate;
    public static event Action OnUpdate;
    public static event Action OnFixedUpdate;
    public static event Action OnGUI;

    private static bool _updated;
    private static bool _calledFixedUpdate;
    private static bool _calledPreUpdate;

    // calls awake before any other script
    internal static void InvokeAwake()
    {
        OnAwake?.Invoke();
    }

    // calls onEnable before any other script
    internal static void InvokeOnEnable()
    {
        OnEnable?.Invoke();
    }

    // calls start before any other script
    internal static void InvokeStart()
    {
        OnStart?.Invoke();
    }

    public static void InvokeUpdate()
    {
        if (_updated) return;
        _updated = true;

        _calledFixedUpdate = false;

        InvokeCallOnPreUpdate();

        OnUpdate?.Invoke();
    }

    // right now I don't call this update before other scripts so I don't need to check if it was already called
    internal static void InvokeLateUpdate()
    {
        _updated = false;
        _calledPreUpdate = false;
    }

    public static void InvokeFixedUpdate()
    {
        if (_calledFixedUpdate) return;
        _calledFixedUpdate = true;

        InvokeCallOnPreUpdate();

        OnFixedUpdate?.Invoke();
    }

    internal static void InvokeOnGUI()
    {
        // currently, this doesn't get called before other scripts
        OnGUI?.Invoke();
    }

    private static void InvokeCallOnPreUpdate()
    {
        if (_calledPreUpdate) return;
        _calledPreUpdate = true;

        OnPreUpdate?.Invoke();
    }
}