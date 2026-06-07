using System;
using System.Linq;
using HarmonyLib;
using MonoMod.Utils;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Patches.Preloader;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using MouseButton = UniTAS.Patcher.Models.VirtualEnvironment.MouseButton;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
[ForceInstantiate]
public class InputSystemOverride
{
    private readonly ILogger _logger;
    private readonly IUpdateEvents _updateEvents;
    private readonly IPatchReverseInvoker _reverseInvoker;
    private readonly IMouseState _mouseState;
    private readonly IKeyboardStateNew _keyboardState;

    private bool _overridden;

    private readonly Action<int, string> NotifyDeviceDiscovered;

    private InputDevice _mouse;
    private InputDevice _keyboard;

    public InputSystemOverride(ILogger logger, IInputSystemState newInputSystemExists, IUpdateEvents updateEvents, IMovieRunnerEvents movieRunnerEvents, IGameRestart gameRestart, IPatchReverseInvoker reverseInvoker, IMouseState mouseState, IKeyboardStateNew keyboardState)
    {
        if (!newInputSystemExists.HasNewInputSystem) return;

        _logger = logger;
        _updateEvents = updateEvents;
        _reverseInvoker = reverseInvoker;
        _mouseState = mouseState;
        _keyboardState = keyboardState;

        _updateEvents.OnUpdateActual += UpdateDevices;
        movieRunnerEvents.OnMovieStart += OnMovieStart;
        movieRunnerEvents.OnMovieEnd += OnMovieEnd;
        gameRestart.OnGameRestartResume += OnGameRestartResume;

        NotifyDeviceDiscovered = AccessTools.Method("UnityEngineInternal.Input.NativeInputSystem:NotifyDeviceDiscovered").CreateDelegate<Action<int, string>>();
    }

    private void LogConnectedDevices()
    {
        _logger.LogDebug(
            $"Connected devices:\n{InputSystem.devices.Select(x => $"name: {x.name}, type: {x.GetType().FullName}").Join()}");
    }

    private void UpdateDevices()
    {
        if (!_overridden) return;

        UpdateMouse();
        UpdateKeyboard();
    }

    private void UpdateMouse()
    {
        ushort buttons = 0;
        if (_mouseState.IsButtonHeld(MouseButton.Left))
        {
            buttons |= 0b1;
        }

        if (_mouseState.IsButtonHeld(MouseButton.Right))
        {
            buttons |= 0b10;
        }

        if (_mouseState.IsButtonHeld(MouseButton.Middle))
        {
            buttons |= 0b100;
        }

        var state = new MouseState
        {
            buttons = buttons,
            position = _mouseState.Position,
            delta = _mouseState.Delta,
            scroll = _mouseState.Scroll,
            // TODO: testing with normal game shows its 0 regardless of how fast I click, test more to determine whats best
            // clickCount = ??,
            // displayIndex = ??, // TODO: probably look into it once virtual env gets more enriched with os stuff
        };

        QueueStateEvent(_mouse, state);
    }

    private void UpdateKeyboard()
    {
        var state = new KeyboardState();
        foreach (var heldKey in _keyboardState.HeldKeys)
        {
            state.Set(heldKey.Key, true);
        }

        QueueStateEvent(_keyboard, state);
    }

    private void QueueStateEvent<TState>(InputDevice device, TState state)
        where TState : struct, IInputStateTypeInfo
    {
        // time is explicitly set to 0 as to queue the event as soon as possible
        InputSystem.QueueStateEvent(device, state, 0.0);
    }

    private void OnMovieStart()
    {
        _overridden = true;
    }

    private void OnMovieEnd()
    {
        _overridden = false;

        RestoreNativeDevices();
        LogConnectedDevices();
    }

    private void OnGameRestartResume(DateTime _, bool preMonoBehaviourResume)
    {
        if (preMonoBehaviourResume) return;

        _mouse = InputSystem.AddDevice<TASMouse>();
        _keyboard = InputSystem.AddDevice<TASKeyboard>();
        _mouse.MakeCurrent();
        _keyboard.MakeCurrent();
    }

    private void RestoreNativeDevices()
    {
        _reverseInvoker.Invoke(this_ =>
        {
            foreach (var device in NewInputSystemPatch.NotifyDeviceDiscovered)
            {
                this_.NotifyDeviceDiscovered(device.DeviceId, device.DeviceDescriptor);
            }
        }, this);
    }

    [InputControlLayout(stateType = typeof(MouseState), isGenericTypeOfDevice = true)]
    private class TASMouse : Mouse;

    [InputControlLayout(stateType = typeof(KeyboardState), isGenericTypeOfDevice = true)]
    private class TASKeyboard : Keyboard;
}
