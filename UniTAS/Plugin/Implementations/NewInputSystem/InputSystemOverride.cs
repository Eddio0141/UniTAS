using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.InputSystemOverride;
using UniTAS.Plugin.Services.InputSystemOverride;
using UniTAS.Plugin.Services.Logging;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UnityEngine.InputSystem;

namespace UniTAS.Plugin.Implementations.NewInputSystem;

[Singleton]
[ForceInstantiate]
public class InputSystemOverride
{
    private readonly InputOverrideDevice[] _devices;
    private InputDevice[] _restoreDevices;

    private readonly ILogger _logger;
    private readonly IVirtualEnvController _virtualEnvController;

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public InputSystemOverride(ILogger logger, InputOverrideDevice[] devices,
        IVirtualEnvController virtualEnvController, INewInputSystemExists newInputSystemExists)
    {
        _logger = logger;
        _devices = devices;
        _virtualEnvController = virtualEnvController;

        var hasInputSystem = newInputSystemExists.HasInputSystem;
        _logger.LogInfo($"InputSystemOverride hasInputSystem: {hasInputSystem}");

        if (!hasInputSystem) return;
        _virtualEnvController.OnVirtualEnvStatusChange += OnVirtualEnvStatusChange;
    }

    private void OnVirtualEnvStatusChange(bool runVirtualEnv)
    {
        if (runVirtualEnv)
        {
            _logger.LogDebug("Adding TAS devices to InputSystem");

            _restoreDevices = InputSystem.devices.ToArray();
            RemoveAndFlushAllDevices();

            foreach (var device in _devices)
            {
                device.DeviceAdded();
            }

            _logger.LogDebug("Added TAS devices to InputSystem");
        }
        else
        {
            _logger.LogDebug("Removing TAS devices from InputSystem");

            RemoveAndFlushAllDevices();

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

    private static void RemoveAndFlushAllDevices()
    {
        while (InputSystem.devices.Count > 0)
        {
            InputSystem.RemoveDevice(InputSystem.devices[0]);
        }

        InputSystem.FlushDisconnectedDevices();
    }
}