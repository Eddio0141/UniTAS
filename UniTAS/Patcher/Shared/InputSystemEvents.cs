using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Shared;

public static class InputSystemEvents
{
    public static event Action OnInputUpdateActual;

    private static bool _usingMonoBehUpdate;
    private static bool _usingMonoBehFixedUpdate;

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public static void Init()
    {
        MonoBehaviourEvents.OnUpdateActual += InputUpdate;
        MonoBehaviourEvents.OnFixedUpdateActual += InputUpdate;

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
                    InputSystem.onBeforeUpdate += InputUpdate;
                }

                if (!_usingMonoBehFixedUpdate)
                {
                    MonoBehaviourEvents.OnFixedUpdateActual += InputUpdate;
                    _usingMonoBehFixedUpdate = true;
                }

                if (_usingMonoBehUpdate)
                {
                    MonoBehaviourEvents.OnUpdateActual -= InputUpdate;
                    _usingMonoBehUpdate = false;
                }

                break;
            case InputSettings.UpdateMode.ProcessEventsInFixedUpdate:
                if (!AlreadyRegisteredOnEvent)
                {
                    InputSystem.onBeforeUpdate += InputUpdate;
                }

                if (!_usingMonoBehUpdate)
                {
                    MonoBehaviourEvents.OnUpdateActual += InputUpdate;
                    _usingMonoBehUpdate = true;
                }

                if (_usingMonoBehFixedUpdate)
                {
                    MonoBehaviourEvents.OnFixedUpdateActual -= InputUpdate;
                    _usingMonoBehFixedUpdate = false;
                }

                break;
            case InputSettings.UpdateMode.ProcessEventsManually:
                if (AlreadyRegisteredOnEvent)
                {
                    InputSystem.onBeforeUpdate -= InputUpdate;
                }

                if (!_usingMonoBehUpdate)
                {
                    MonoBehaviourEvents.OnUpdateActual += InputUpdate;
                    _usingMonoBehUpdate = true;
                }
                else if (!_usingMonoBehFixedUpdate)
                {
                    MonoBehaviourEvents.OnFixedUpdateActual += InputUpdate;
                    _usingMonoBehFixedUpdate = true;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(updateMode), updateMode, null);
        }
    }

    private static bool AlreadyRegisteredOnEvent => !_usingMonoBehUpdate || !_usingMonoBehFixedUpdate;

    public static void InputUpdate()
    {
        if (!MonoBehaviourController.PausedExecution) OnInputUpdateActual?.Invoke();
    }
}