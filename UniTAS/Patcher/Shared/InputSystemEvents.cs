using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Shared;

public static class InputSystemEvents
{
    public delegate void InputUpdateActual(bool fixedUpdate, bool newInputSystemUpdate);

    public static event InputUpdateActual OnInputUpdateActual;

    private static bool _usingMonoBehUpdate;
    private static bool _usingMonoBehFixedUpdate;

    private static bool _alreadyRegisteredOnEvent;

    private static Action _inputUpdateActual;
    private static Action _inputUpdateFixedUpdate;

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public static void Init()
    {
        if (_usingMonoBehUpdate)
        {
            MonoBehaviourEvents.OnUpdateActual -= _inputUpdateActual;
        }
        else if (_alreadyRegisteredOnEvent)
        {
            InputSystem.onBeforeUpdate -= _inputUpdateActual;
        }

        if (_usingMonoBehFixedUpdate)
        {
            MonoBehaviourEvents.OnFixedUpdateActual -= _inputUpdateFixedUpdate;
        }
        else if (_alreadyRegisteredOnEvent)
        {
            InputSystem.onBeforeUpdate -= _inputUpdateFixedUpdate;
        }

        _usingMonoBehUpdate = true;
        _usingMonoBehFixedUpdate = true;

        _inputUpdateActual = () => InputUpdate(false, false);
        MonoBehaviourEvents.OnUpdateActual += _inputUpdateActual;
        _inputUpdateFixedUpdate = () => InputUpdate(true, false);
        MonoBehaviourEvents.OnFixedUpdateActual += _inputUpdateFixedUpdate;

        var hasInputSystem = false;
        try
        {
            if (Mouse.current != null)
            {
                // check dummy
            }

            hasInputSystem = true;
        }
        catch (Exception)
        {
            // ignored
        }

        if (!hasInputSystem) return;

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

        _alreadyRegisteredOnEvent = true;
    }

    private static bool AlreadyRegisteredOnEvent => !_usingMonoBehUpdate || !_usingMonoBehFixedUpdate;

    private static void InputUpdate(bool fixedUpdate, bool newInputSystemUpdate)
    {
        if (!MonoBehaviourController.PausedExecution) OnInputUpdateActual?.Invoke(fixedUpdate, newInputSystemUpdate);
    }
}