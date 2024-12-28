using System;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Models.Utils;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.UnityEvents;
using UnityEngine.InputSystem;
#if TRACE
using UniTAS.Patcher.Utils;
using UniTAS.Patcher.Services;
using UnityEngine;
#endif

namespace UniTAS.Patcher.Implementations.UnityEvents;

public partial class UnityEvents
{
    private readonly IInputSystemState _newInputSystemExists;

    private void InputSystemEventsInit()
    {
        if (_usingMonoBehUpdate)
        {
            OnUpdateUnconditional -= _inputUpdate;
        }
        // else if (_alreadyRegisteredOnEvent)
        // {
        //     InputSystem.onBeforeUpdate -= _inputUpdateActual;
        // }

        if (_usingMonoBehFixedUpdate)
        {
            OnFixedUpdateUnconditional -= _inputFixedUpdate;
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

        if (!_newInputSystemExists.HasNewInputSystem) return;

        InputSystemChangeUpdate(InputSystem.settings.updateMode);
    }

    private void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (!preSceneLoad) return;
        InputSystemEventsInit();
    }

    public event IUpdateEvents.InputUpdateCall OnInputUpdateActual
    {
        add => _inputUpdatesActual.Add(value, (int)CallbackPriority.Default);
        remove => _inputUpdatesActual.Remove(value);
    }

    public event IUpdateEvents.InputUpdateCall OnInputUpdateUnconditional
    {
        add => _inputUpdatesUnconditional.Add(value, (int)CallbackPriority.Default);
        remove => _inputUpdatesUnconditional.Remove(value);
    }

    public void AddPriorityCallback(CallbackInputUpdate callbackUpdate, IUpdateEvents.InputUpdateCall callback,
        CallbackPriority priority)
    {
        var callbackList = callbackUpdate switch
        {
            CallbackInputUpdate.InputUpdateActual => _inputUpdatesActual,
            CallbackInputUpdate.InputUpdateUnconditional => _inputUpdatesUnconditional,
            _ => throw new ArgumentOutOfRangeException(nameof(callbackUpdate), callbackUpdate, null)
        };

        callbackList.Add(callback, (int)priority);
    }

    private readonly PriorityList<IUpdateEvents.InputUpdateCall> _inputUpdatesActual = new();
    private readonly PriorityList<IUpdateEvents.InputUpdateCall> _inputUpdatesUnconditional = new();

    private bool _usingMonoBehUpdate;
    private bool _usingMonoBehFixedUpdate;

    // private bool _alreadyRegisteredOnEvent;

    private Action _inputUpdate;
    private Action _inputFixedUpdate;

    private void AddEventToFixedUpdateUnconditional()
    {
        AddPriorityCallback(CallbackUpdate.FixedUpdateUnconditional, _inputFixedUpdate, CallbackPriority.InputUpdate);
    }

    private void AddEventToUpdateUnconditional()
    {
        AddPriorityCallback(CallbackUpdate.UpdateUnconditional, _inputUpdate, CallbackPriority.InputUpdate);
    }

    public void InputSystemChangeUpdate(InputSettings.UpdateMode updateMode)
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
                    OnUpdateUnconditional -= _inputUpdate;
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
                    OnFixedUpdateUnconditional -= _inputFixedUpdate;
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

    private bool AlreadyRegisteredOnEvent => !_usingMonoBehUpdate || !_usingMonoBehFixedUpdate;

    private void InputUpdate(bool fixedUpdate, bool newInputSystemUpdate)
    {
#if TRACE
        StaticLogger.Trace(
            $"InputUpdate, time: {_patchReverseInvoker.Invoke(() => Time.frameCount)} fixed update: {fixedUpdate}, new input system update: {newInputSystemUpdate}");
#endif

        for (var i = 0; i < _inputUpdatesUnconditional.Count; i++)
        {
            _inputUpdatesUnconditional[i](fixedUpdate, newInputSystemUpdate);
        }

        for (var i = 0; i < _inputUpdatesActual.Count; i++)
        {
            if (_monoBehaviourController.PausedExecution || (!fixedUpdate && _monoBehaviourController.PausedUpdate))
                continue;
            _inputUpdatesActual[i](fixedUpdate, newInputSystemUpdate);
        }
    }
}