using System;
using System.Collections.Generic;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.InputSystemOverride;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
[ForceInstantiate]
public class InputSystemOverride : IInputSystemTrackerUpdate
{
    private readonly InputOverrideDevice[] _devices;
    private readonly ILogger _logger;
    private readonly List<InputDevice> _actualDevices = new();
    private readonly IUpdateEvents _updateEvents;

    private bool _overridden;

    public InputSystemOverride(ILogger logger, InputOverrideDevice[] devices,
        IInputSystemState newInputSystemExists, IUpdateEvents updateEvents, IVirtualEnvController virtualEnv,
        IMovieRunner movieRunner, IGameRestart gameRestart)
    {
        if (!newInputSystemExists.HasNewInputSystem) return;

        _logger = logger;
        _devices = devices;
        _updateEvents = updateEvents;

        _updateEvents.OnUpdateActual += UpdateDevices;
        virtualEnv.OnVirtualEnvStatusChange += OnVirtualEnvStatusChange;
        gameRestart.OnPreGameRestart += OnPreGameRestart;

        _actualDevices.AddRange(InputSystem.devices);
    }

    private void OnVirtualEnvStatusChange(bool runVirtualEnv)
    {
        _overridden = runVirtualEnv;

        if (runVirtualEnv)
        {
            foreach (var device in _actualDevices)
            {
                InputSystem.RemoveDevice(device);
            }

            _logger.LogDebug("removed all InputSystem devices that isn't ours");
            LogConnectedDevices();

            foreach (var device in _devices)
            {
                device.AddDevice();
                device.MakeCurrent();
            }

            _logger.LogDebug("adding TAS devices to InputSystem");
            LogConnectedDevices();
        }
        else
        {
            foreach (var device in _devices)
            {
                device.RemoveDevice();
            }

            _logger.LogDebug("removed all TAS devices from InputSystem");
            LogConnectedDevices();

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

    private void UpdateDevices()
    {
        if (!_overridden) return;

        foreach (var device in _devices)
        {
            device.Update();
        }
    }

    private readonly List<Action> _onBeforeUpdateEvents = [];

    public void NewOnBeforeUpdateEvent(Action action) => _onBeforeUpdateEvents.Add(action);

    private void OnPreGameRestart()
    {
        _logger.LogDebug($"unsubscribing {_onBeforeUpdateEvents.Count} onBeforeUpdate events");
        foreach (var beforeUpdate in _onBeforeUpdateEvents)
        {
            _logger.LogDebug($"thingy: {beforeUpdate.Method.DeclaringType?.FullName}.{beforeUpdate.Method.Name}");
            InputSystem.onBeforeUpdate -= beforeUpdate;
        }
    }
}