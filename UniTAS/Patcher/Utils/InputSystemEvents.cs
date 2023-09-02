using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.Invoker;
using UniTAS.Patcher.Models;
using UniTAS.Patcher.Models.EventSubscribers;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Utils;

public static class InputSystemEvents
{
    public delegate void InputUpdateCall(bool fixedUpdate, bool newInputSystemUpdate);

    public static event InputUpdateCall OnInputUpdateActual
    {
        add => InputUpdatesActual.Add(value, (int)CallbackPriority.Default);
        remove => InputUpdatesActual.Remove(value);
    }

    public static event InputUpdateCall OnInputUpdateUnconditional
    {
        add => InputUpdatesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => InputUpdatesUnconditional.Remove(value);
    }

    public static readonly PriorityList<InputUpdateCall> InputUpdatesActual = new();
    public static readonly PriorityList<InputUpdateCall> InputUpdatesUnconditional = new();

    private static bool _usingMonoBehUpdate;
    private static bool _usingMonoBehFixedUpdate;

    // private static bool _alreadyRegisteredOnEvent;

    private static Action _inputUpdateActual;
    private static Action _inputUpdateFixedUpdate;

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    [InvokeOnUnityInit]
    public static void Init()
    {
        if (_usingMonoBehUpdate)
        {
            MonoBehaviourEvents.OnUpdateActual -= _inputUpdateActual;
        }
        // else if (_alreadyRegisteredOnEvent)
        // {
        //     InputSystem.onBeforeUpdate -= _inputUpdateActual;
        // }

        if (_usingMonoBehFixedUpdate)
        {
            MonoBehaviourEvents.OnFixedUpdateActual -= _inputUpdateFixedUpdate;
        }
        // else if (_alreadyRegisteredOnEvent)
        // {
        //     InputSystem.onBeforeUpdate -= _inputUpdateFixedUpdate;
        // }

        _usingMonoBehUpdate = true;
        _usingMonoBehFixedUpdate = true;

        _inputUpdateActual = () => InputUpdate(false, false);
        MonoBehaviourEvents.OnUpdateActual += _inputUpdateActual;
        _inputUpdateFixedUpdate = () => InputUpdate(true, false);
        MonoBehaviourEvents.OnFixedUpdateActual += _inputUpdateFixedUpdate;

        if (!NewInputSystemState.NewInputSystemExists) return;

        InputSystemChangeUpdate(InputSystem.settings.updateMode);
    }

    public static void InputSystemChangeUpdate(InputSettings.UpdateMode updateMode)
    {
        switch (updateMode)
        {
            case InputSettings.UpdateMode.ProcessEventsInDynamicUpdate:
            {
                var registerOnBeforeUpdate = !AlreadyRegisteredOnEvent;

                if (!_usingMonoBehFixedUpdate)
                {
                    _inputUpdateFixedUpdate = () => InputUpdate(true, false);
                    MonoBehaviourEvents.OnFixedUpdateActual += _inputUpdateFixedUpdate;
                    _usingMonoBehFixedUpdate = true;
                }

                if (_usingMonoBehUpdate)
                {
                    MonoBehaviourEvents.OnUpdateActual -= _inputUpdateActual;
                    _usingMonoBehUpdate = false;
                }

                if (registerOnBeforeUpdate)
                {
                    _inputUpdateActual = () => InputUpdate(false, true);
                    InputSystem.onBeforeUpdate += _inputUpdateActual;
                }

                break;
            }
            case InputSettings.UpdateMode.ProcessEventsInFixedUpdate:
            {
                var registerOnBeforeUpdate = !AlreadyRegisteredOnEvent;

                if (!_usingMonoBehUpdate)
                {
                    _inputUpdateActual = () => InputUpdate(false, false);
                    MonoBehaviourEvents.OnUpdateActual += _inputUpdateActual;
                    _usingMonoBehUpdate = true;
                }

                if (_usingMonoBehFixedUpdate)
                {
                    MonoBehaviourEvents.OnFixedUpdateActual -= _inputUpdateFixedUpdate;
                    _usingMonoBehFixedUpdate = false;
                }

                if (registerOnBeforeUpdate)
                {
                    _inputUpdateFixedUpdate = () => InputUpdate(true, true);
                    InputSystem.onBeforeUpdate += _inputUpdateFixedUpdate;
                }

                break;
            }
            case InputSettings.UpdateMode.ProcessEventsManually:
                if (AlreadyRegisteredOnEvent)
                {
                    InputSystem.onBeforeUpdate -= _usingMonoBehUpdate ? _inputUpdateFixedUpdate : _inputUpdateActual;
                }

                if (!_usingMonoBehUpdate)
                {
                    _inputUpdateActual = () => InputUpdate(false, false);
                    MonoBehaviourEvents.OnUpdateActual += _inputUpdateActual;
                    _usingMonoBehUpdate = true;
                }

                if (!_usingMonoBehFixedUpdate)
                {
                    _inputUpdateFixedUpdate = () => InputUpdate(true, false);
                    MonoBehaviourEvents.OnFixedUpdateActual += _inputUpdateFixedUpdate;
                    _usingMonoBehFixedUpdate = true;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(updateMode), updateMode, null);
        }

        // _alreadyRegisteredOnEvent = true;
    }

    private static bool AlreadyRegisteredOnEvent => !_usingMonoBehUpdate || !_usingMonoBehFixedUpdate;

    private static void InputUpdate(bool fixedUpdate, bool newInputSystemUpdate)
    {
        StaticLogger.Trace(
            $"InputUpdate, fixed update: {fixedUpdate}, new input system update: {newInputSystemUpdate}");
        for (var i = 0; i < InputUpdatesUnconditional.Count; i++)
        {
            InputUpdatesUnconditional[i](fixedUpdate, newInputSystemUpdate);
        }

        for (var i = 0; i < InputUpdatesActual.Count; i++)
        {
            if (MonoBehaviourController.PausedExecution ||
                (!fixedUpdate && MonoBehaviourController.PausedUpdate)) continue;
            InputUpdatesActual[i](fixedUpdate, newInputSystemUpdate);
        }
    }
}