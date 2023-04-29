using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Shared;

public static class InputSystemEvents
{
    public static event Action<bool> OnInputUpdateActual;

    private static bool _usingMonoBehUpdate;
    private static bool _usingMonoBehFixedUpdate;

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public static void Init()
    {
        MonoBehaviourEvents.OnUpdateActual += InputUpdateNonFixedUpdate;
        MonoBehaviourEvents.OnFixedUpdateActual += InputUpdateFixedUpdate;

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

        _usingMonoBehUpdate = true;
        _usingMonoBehFixedUpdate = true;
        InputSystemChangeUpdate(InputSystem.settings.updateMode);
    }

    public static void InputSystemChangeUpdate(InputSettings.UpdateMode updateMode)
    {
        switch (updateMode)
        {
            case InputSettings.UpdateMode.ProcessEventsInDynamicUpdate:
                if (!AlreadyRegisteredOnEvent)
                {
                    InputSystem.onBeforeUpdate += InputUpdateNonFixedUpdate;
                }

                if (!_usingMonoBehFixedUpdate)
                {
                    MonoBehaviourEvents.OnFixedUpdateActual += InputUpdateFixedUpdate;
                    _usingMonoBehFixedUpdate = true;
                }

                if (_usingMonoBehUpdate)
                {
                    MonoBehaviourEvents.OnUpdateActual -= InputUpdateNonFixedUpdate;
                    _usingMonoBehUpdate = false;
                }

                break;
            case InputSettings.UpdateMode.ProcessEventsInFixedUpdate:
                if (!AlreadyRegisteredOnEvent)
                {
                    InputSystem.onBeforeUpdate += InputUpdateFixedUpdate;
                }

                if (!_usingMonoBehUpdate)
                {
                    MonoBehaviourEvents.OnUpdateActual += InputUpdateNonFixedUpdate;
                    _usingMonoBehUpdate = true;
                }

                if (_usingMonoBehFixedUpdate)
                {
                    MonoBehaviourEvents.OnFixedUpdateActual -= InputUpdateFixedUpdate;
                    _usingMonoBehFixedUpdate = false;
                }

                break;
            case InputSettings.UpdateMode.ProcessEventsManually:
                if (AlreadyRegisteredOnEvent)
                {
                    InputSystem.onBeforeUpdate -=
                        _usingMonoBehUpdate ? InputUpdateFixedUpdate : InputUpdateNonFixedUpdate;
                }

                if (!_usingMonoBehUpdate)
                {
                    MonoBehaviourEvents.OnUpdateActual += InputUpdateNonFixedUpdate;
                    _usingMonoBehUpdate = true;
                }
                else if (!_usingMonoBehFixedUpdate)
                {
                    MonoBehaviourEvents.OnFixedUpdateActual += InputUpdateFixedUpdate;
                    _usingMonoBehFixedUpdate = true;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(updateMode), updateMode, null);
        }
    }

    private static bool AlreadyRegisteredOnEvent => !_usingMonoBehUpdate || !_usingMonoBehFixedUpdate;

    private static void InputUpdateNonFixedUpdate()
    {
        if (!MonoBehaviourController.PausedExecution) OnInputUpdateActual?.Invoke(false);
    }

    private static void InputUpdateFixedUpdate()
    {
        if (!MonoBehaviourController.PausedExecution) OnInputUpdateActual?.Invoke(true);
    }
}