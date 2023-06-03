using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.InputSystemOverride;
using UniTAS.Patcher.Services.EventSubscribers;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.NewInputSystem;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
[ForceInstantiate]
public class InputSystemOverride : IInputSystemOverride
{
    private readonly IInputOverrideDevice[] _devices;
    private InputDevice[] _restoreDevices;

    private readonly ILogger _logger;
    private readonly bool _hasInputSystem;

    private bool _override;

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public InputSystemOverride(ILogger logger, IInputOverrideDevice[] devices,
        INewInputSystemExists newInputSystemExists, IUpdateEvents updateEvents)
    {
        _logger = logger;
        _devices = devices;

        _hasInputSystem = newInputSystemExists.HasInputSystem;
        _logger.LogMessage($"Has unity new input system: {_hasInputSystem}");

        if (!_hasInputSystem) return;

        updateEvents.OnInputUpdateActual += UpdateDevices;
    }

    public bool Override
    {
        set
        {
            if (!_hasInputSystem) return;

            _override = value;
            if (_override)
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
    }

    private void UpdateDevices(bool fixedUpdate, bool newInputSystemUpdate)
    {
        if (!_override || !newInputSystemUpdate) return;

        foreach (var device in _devices)
        {
            device.Update();
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