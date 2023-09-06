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

    private static Action _inputUpdate;
    private static Action _inputFixedUpdate;

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    [InvokeOnUnityInit]
    public static void Init()
    {
        if (_usingMonoBehUpdate)
        {
            MonoBehaviourEvents.OnUpdateUnconditional -= _inputUpdate;
        }
        // else if (_alreadyRegisteredOnEvent)
        // {
        //     InputSystem.onBeforeUpdate -= _inputUpdateActual;
        // }

        if (_usingMonoBehFixedUpdate)
        {
            MonoBehaviourEvents.OnFixedUpdateUnconditional -= _inputFixedUpdate;
        }
        // else if (_alreadyRegisteredOnEvent)
        // {
        //     InputSystem.onBeforeUpdate -= _inputUpdateFixedUpdate;
        // }

        _usingMonoBehUpdate = true;
        _usingMonoBehFixedUpdate = true;

        _inputUpdate = () => InputUpdate(false, false);
        AddEventToUpdateUnconditional();
        _inputFixedUpdate = () => InputUpdate(true, false);
        AddEventToFixedUpdateUnconditional();

        if (!NewInputSystemState.NewInputSystemExists) return;

        InputSystemChangeUpdate(InputSystem.settings.updateMode);
    }

    private static void AddEventToFixedUpdateUnconditional()
    {
        MonoBehaviourEvents.FixedUpdatesUnconditional.Add(_inputFixedUpdate,
            (int)CallbackPriority.InputUpdate);
    }

    private static void AddEventToUpdateUnconditional()
    {
        MonoBehaviourEvents.UpdatesUnconditional.Add(_inputUpdate,
            (int)CallbackPriority.InputUpdate);
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
                    _inputFixedUpdate = () => InputUpdate(true, false);
                    AddEventToFixedUpdateUnconditional();
                    _usingMonoBehFixedUpdate = true;
                }

                if (_usingMonoBehUpdate)
                {
                    MonoBehaviourEvents.OnUpdateUnconditional -= _inputUpdate;
                    _usingMonoBehUpdate = false;
                }

                if (registerOnBeforeUpdate)
                {
                    _inputUpdate = () => InputUpdate(false, true);
                    InputSystem.onBeforeUpdate += _inputUpdate;
                }

                break;
            }
            case InputSettings.UpdateMode.ProcessEventsInFixedUpdate:
            {
                var registerOnBeforeUpdate = !AlreadyRegisteredOnEvent;

                if (!_usingMonoBehUpdate)
                {
                    _inputUpdate = () => InputUpdate(false, false);
                    AddEventToUpdateUnconditional();
                    _usingMonoBehUpdate = true;
                }

                if (_usingMonoBehFixedUpdate)
                {
                    MonoBehaviourEvents.OnFixedUpdateUnconditional -= _inputFixedUpdate;
                    _usingMonoBehFixedUpdate = false;
                }

                if (registerOnBeforeUpdate)
                {
                    _inputFixedUpdate = () => InputUpdate(true, true);
                    InputSystem.onBeforeUpdate += _inputFixedUpdate;
                }

                break;
            }
            case InputSettings.UpdateMode.ProcessEventsManually:
                if (AlreadyRegisteredOnEvent)
                {
                    InputSystem.onBeforeUpdate -= _usingMonoBehUpdate ? _inputFixedUpdate : _inputUpdate;
                }

                if (!_usingMonoBehUpdate)
                {
                    _inputUpdate = () => InputUpdate(false, false);
                    AddEventToUpdateUnconditional();
                    _usingMonoBehUpdate = true;
                }

                if (!_usingMonoBehFixedUpdate)
                {
                    _inputFixedUpdate = () => InputUpdate(true, false);
                    AddEventToFixedUpdateUnconditional();
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