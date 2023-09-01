using System;
using UniTAS.Patcher.Models;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Services;
using UnityEngine;

namespace UniTAS.Patcher.Utils;

/// <summary>
/// Event calls for MonoBehaviour event methods
/// Unconditional will be called before Actual
/// Unconditional events are called even if MonoBehaviour is paused
/// Actual events are called only if MonoBehaviour is not paused
/// </summary>
public static class MonoBehaviourEvents
{
    public static event Action OnAwakeUnconditional
    {
        add => AwakesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => AwakesUnconditional.Remove(value);
    }

    public static event Action OnAwakeActual
    {
        add => AwakesActual.Add(value, (int)CallbackPriority.Default);
        remove => AwakesActual.Remove(value);
    }

    public static event Action OnStartUnconditional
    {
        add => StartsUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => StartsUnconditional.Remove(value);
    }

    public static event Action OnStartActual
    {
        add => StartsActual.Add(value, (int)CallbackPriority.Default);
        remove => StartsActual.Remove(value);
    }

    public static event Action OnEnableUnconditional
    {
        add => EnablesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => EnablesUnconditional.Remove(value);
    }

    public static event Action OnEnableActual
    {
        add => EnablesActual.Add(value, (int)CallbackPriority.Default);
        remove => EnablesActual.Remove(value);
    }

    public static event Action OnPreUpdateUnconditional
    {
        add => PreUpdatesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => PreUpdatesUnconditional.Remove(value);
    }

    public static event Action OnPreUpdateActual
    {
        add => PreUpdatesActual.Add(value, (int)CallbackPriority.Default);
        remove => PreUpdatesActual.Remove(value);
    }

    public static event Action OnUpdateUnconditional
    {
        add => UpdatesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => UpdatesUnconditional.Remove(value);
    }

    public static event Action OnUpdateActual
    {
        add => UpdatesActual.Add(value, (int)CallbackPriority.Default);
        remove => UpdatesActual.Remove(value);
    }

    public static event Action OnFixedUpdateUnconditional
    {
        add => FixedUpdatesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => FixedUpdatesUnconditional.Remove(value);
    }

    public static event Action OnFixedUpdateActual
    {
        add => FixedUpdatesActual.Add(value, (int)CallbackPriority.Default);
        remove => FixedUpdatesActual.Remove(value);
    }

    public static event Action OnGUIUnconditional
    {
        add => GUIsUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => GUIsUnconditional.Remove(value);
    }

    public static event Action OnGUIActual
    {
        add => GUIsActual.Add(value, (int)CallbackPriority.Default);
        remove => GUIsActual.Remove(value);
    }

    public static event Action OnLastUpdateUnconditional
    {
        add => LastUpdatesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => LastUpdatesUnconditional.Remove(value);
    }

    public static event Action OnLastUpdateActual
    {
        add => LastUpdatesActual.Add(value, (int)CallbackPriority.Default);
        remove => LastUpdatesActual.Remove(value);
    }

    // if touching this, probably should rewrite but otherwise i ain't doing anything since its not like there's much to add
    public static readonly PriorityList<Action> AwakesUnconditional = new();
    public static readonly PriorityList<Action> AwakesActual = new();

    public static readonly PriorityList<Action> StartsUnconditional = new();
    public static readonly PriorityList<Action> StartsActual = new();

    public static readonly PriorityList<Action> EnablesUnconditional = new();
    public static readonly PriorityList<Action> EnablesActual = new();

    public static readonly PriorityList<Action> PreUpdatesUnconditional = new();
    public static readonly PriorityList<Action> PreUpdatesActual = new();

    public static readonly PriorityList<Action> UpdatesUnconditional = new();
    public static readonly PriorityList<Action> UpdatesActual = new();

    public static readonly PriorityList<Action> FixedUpdatesUnconditional = new();
    public static readonly PriorityList<Action> FixedUpdatesActual = new();

    public static readonly PriorityList<Action> GUIsUnconditional = new();
    public static readonly PriorityList<Action> GUIsActual = new();

    public static readonly PriorityList<Action> LastUpdatesUnconditional = new();
    public static readonly PriorityList<Action> LastUpdatesActual = new();

    private static bool _updated;
    private static bool _calledFixedUpdate;
    private static bool _calledPreUpdate;

    public static void InvokeLastUpdate()
    {
        var unconditionalCount = LastUpdatesUnconditional.Count;
        for (var i = 0; i < unconditionalCount; i++)
        {
            LastUpdatesUnconditional[i]();
        }

        var actualCount = LastUpdatesActual.Count;
        for (var i = 0; i < actualCount; i++)
        {
            var lastUpdate = LastUpdatesActual[i];
            if (MonoBehaviourController.PausedExecution || MonoBehaviourController.PausedUpdate) continue;
            lastUpdate();
        }
    }

    // calls awake before any other script
    public static void InvokeAwake()
    {
        var unconditionalCount = AwakesUnconditional.Count;
        for (var i = 0; i < unconditionalCount; i++)
        {
            AwakesUnconditional[i]();
        }

        var actualCount = AwakesActual.Count;
        for (var i = 0; i < actualCount; i++)
        {
            var awake = AwakesActual[i];
            if (!MonoBehaviourController.PausedExecution)
                awake();
        }
    }

    // calls onEnable before any other script
    public static void InvokeOnEnable()
    {
        var unconditionalCount = EnablesUnconditional.Count;
        for (var i = 0; i < unconditionalCount; i++)
        {
            EnablesUnconditional[i]();
        }

        var actualCount = EnablesActual.Count;
        for (var i = 0; i < actualCount; i++)
        {
            var enable = EnablesActual[i];
            if (!MonoBehaviourController.PausedExecution)
                enable();
        }
    }

    // calls start before any other script
    public static void InvokeStart()
    {
        OnStartUnconditional?.Invoke();
        if (!MonoBehaviourController.PausedExecution)
            OnStartActual?.Invoke();
    }
        var unconditionalCount = StartsUnconditional.Count;
        for (var i = 0; i < unconditionalCount; i++)
        {
            StartsUnconditional[i]();
        }

        var actualCount = StartsActual.Count;
        for (var i = 0; i < actualCount; i++)
        {
            var start = StartsActual[i];
            if (!MonoBehaviourController.PausedExecution)
                start();
        }
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

        var unconditionalCount = UpdatesUnconditional.Count;
        for (var i = 0; i < unconditionalCount; i++)
        {
            UpdatesUnconditional[i]();
        }

        var actualCount = UpdatesActual.Count;
        for (var i = 0; i < actualCount; i++)
        {
            var update = UpdatesActual[i];
            if (MonoBehaviourController.PausedExecution || MonoBehaviourController.PausedUpdate) continue;
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

        var unconditionalCount = FixedUpdatesUnconditional.Count;
        for (var i = 0; i < unconditionalCount; i++)
        {
            FixedUpdatesUnconditional[i]();
        }

        var actualCount = FixedUpdatesActual.Count;
        for (var i = 0; i < actualCount; i++)
        {
            var fixedUpdate = FixedUpdatesActual[i];
            if (!MonoBehaviourController.PausedExecution)
                fixedUpdate();
        }
    }

    public static void InvokeOnGUI()
    {
        // currently, this doesn't get called before other scripts
        var unconditionalCount = GUIsUnconditional.Count;
        for (var i = 0; i < unconditionalCount; i++)
        {
            GUIsUnconditional[i]();
        }

        var actualCount = GUIsActual.Count;
        for (var i = 0; i < actualCount; i++)
        {
            var gui = GUIsActual[i];
            if (!MonoBehaviourController.PausedExecution)
                gui();
        }
    }

    private static void InvokeCallOnPreUpdate()
    {
        var unconditionalCount = PreUpdatesUnconditional.Count;
        for (var i = 0; i < unconditionalCount; i++)
        {
            PreUpdatesUnconditional[i]();
        }

        var actualCount = PreUpdatesActual.Count;
        for (var i = 0; i < actualCount; i++)
        {
            var preUpdate = PreUpdatesActual[i];
            if (!MonoBehaviourController.PausedExecution)
                preUpdate();
        }
    }
}