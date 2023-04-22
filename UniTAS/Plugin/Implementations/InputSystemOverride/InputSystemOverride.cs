using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;

namespace UniTAS.Plugin.Implementations.InputSystemOverride;

[Singleton]
public class InputSystemOverride : IOnPreUpdatesActual
{
    private readonly bool _hasInputSystem;

    private TASMouse _mouse;
    private TASKeyboard _keyboard;

    private InputDevice[] _restoreDevices;

    private readonly IMouseStateEnv _mouseStateEnv;
    private readonly IKeyboardStateEnv _keyboardStateEnv;
    private readonly IVirtualEnvController _virtualEnvController;

    private readonly ILogger _logger;

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public InputSystemOverride(IMouseStateEnv mouseStateEnv, IKeyboardStateEnv keyboardStateEnv,
        IVirtualEnvController virtualEnvController, ILogger logger)
    {
        _mouseStateEnv = mouseStateEnv;
        _keyboardStateEnv = keyboardStateEnv;
        _virtualEnvController = virtualEnvController;
        _logger = logger;

        try
        {
            if (Mouse.current != null)
            {
                // check dummy
            }

            _hasInputSystem = true;
        }
        catch (Exception)
        {
            // ignored
        }

        _logger.LogInfo($"InputSystemOverride hasInputSystem: {_hasInputSystem}");

        _virtualEnvController.OnVirtualEnvStatusChange += OnVirtualEnvStatusChange;
    }

    [InputControlLayout(stateType = typeof(MouseState), isGenericTypeOfDevice = true)]
    private class TASMouse : Mouse
    {
    }

    [InputControlLayout(stateType = typeof(KeyboardState), isGenericTypeOfDevice = true)]
    private class TASKeyboard : Keyboard
    {
    }

    public void OnVirtualEnvStatusChange(bool runVirtualEnv)
    {
        if (!_hasInputSystem) return;

        if (runVirtualEnv)
        {
            _logger.LogDebug("Adding TAS devices to InputSystem");

            // remove all connected devices
            _restoreDevices = InputSystem.devices.ToArray();
            while (InputSystem.devices.Count > 0)
            {
                InputSystem.RemoveDevice(InputSystem.devices[0]);
            }

            _mouse = InputSystem.AddDevice<TASMouse>();
            _keyboard = InputSystem.AddDevice<TASKeyboard>();
            _logger.LogDebug("Added TAS devices to InputSystem");
        }
        else
        {
            _logger.LogDebug("Removing TAS devices from InputSystem");

            // remove all connected devices
            while (InputSystem.devices.Count > 0)
            {
                InputSystem.RemoveDevice(InputSystem.devices[0]);
            }

            // restore devices
            if (_restoreDevices != null)
            {
                foreach (var device in _restoreDevices)
                {
                    InputSystem.AddDevice(device);
                }
            }

            _logger.LogDebug("Removed TAS devices from InputSystem");
        }
    }

    public void PreUpdateActual()
    {
        if (!_hasInputSystem || !_virtualEnvController.RunVirtualEnvironment) return;

        var state = new MouseState
        {
            // bit 0 = left mouse button
            buttons = (ushort)(_mouseStateEnv.LeftClick ? 1 : 0),
        };

        // var keys = new byte[14];
        // if (_keyboardStateEnv.Keys.Contains(KeyCode.Space)) keys[0] = 1;

        var stateKeyboard = new KeyboardState();
        unsafe
        {
            stateKeyboard.keys[0] = 1;
        }

        InputSystem.QueueStateEvent(_mouse, state);
    }
}