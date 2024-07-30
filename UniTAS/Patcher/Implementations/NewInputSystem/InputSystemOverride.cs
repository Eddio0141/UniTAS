using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.InputSystemOverride;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
[ForceInstantiate]
public class InputSystemOverride
{
    private readonly bool _hasInputSystem;

    private readonly IInputOverrideDevice[] _devices;
    private readonly ILogger _logger;
    private readonly List<InputDevice> _actualDevices = new();
    private readonly IMovieRunner _movieRunner;
    private readonly IUpdateEvents _updateEvents;

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public InputSystemOverride(ILogger logger, IInputOverrideDevice[] devices,
        IInputSystemState newInputSystemExists, IUpdateEvents updateEvents, IVirtualEnvController virtualEnv, IMovieRunner movieRunner)
    {
        _hasInputSystem = newInputSystemExists.HasNewInputSystem;
        logger.LogMessage($"Has unity new input system: {_hasInputSystem}");

        if (!_hasInputSystem) return;

        _logger = logger;
        _devices = devices;
        _movieRunner = movieRunner;
        _updateEvents = updateEvents;
        virtualEnv.OnVirtualEnvStatusChange += OnVirtualEnvStatusChange;

        _actualDevices.AddRange(InputSystem.devices);

        _logger.LogDebug("Adding TAS devices to InputSystem");

        foreach (var device in _devices)
        {
            device.AddDevice();
        }

        LogConnectedDevices();
    }

    private void OnVirtualEnvStatusChange(bool runVirtualEnv)
    {
        if (runVirtualEnv)
        {
            _updateEvents.OnInputUpdateActual += UpdateDevices;

            foreach (var device in _actualDevices)
            {
                InputSystem.RemoveDevice(device);
            }

            _logger.LogDebug("removed all InputSystem devices that isn't ours");
            LogConnectedDevices();

            foreach (var device in _devices)
            {
                device.Device.MakeCurrent();
            }
        }
        else
        {
            _updateEvents.OnInputUpdateActual -= UpdateDevices;

            foreach (var device in _actualDevices)
            {
                InputSystem.AddDevice(device);
                device.MakeCurrent();
            }

            _logger.LogDebug("restored all InputSystem devices that isn't ours");
            LogConnectedDevices();
        }
    }

    private void LogConnectedDevices()
    {
        _logger.LogDebug(
            $"Connected devices:\n{string.Join("\n", InputSystem.devices.Select(x => $"name: {x.name}, type: {x.GetType().FullName}").ToArray())}");
    }

    private void UpdateDevices(bool fixedUpdate, bool newInputSystemUpdate)
    {
        if (!newInputSystemUpdate) return;

        foreach (var device in _devices)
        {
            device.Update();
        }
    }
}
