using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.InputSystemOverride;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Services.UnityEvents;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
[ForceInstantiate]
public class InputSystemOverride
{
    private readonly InputOverrideDevice[] _devices;
    private readonly ILogger _logger;
    private readonly List<InputDevice> _actualDevices = [];
    private readonly IUpdateEvents _updateEvents;

    private bool _overridden;

    public InputSystemOverride(ILogger logger, InputOverrideDevice[] devices, IInputSystemState newInputSystemExists, IUpdateEvents updateEvents, IMovieRunnerEvents movieRunnerEvents)
    {
        if (!newInputSystemExists.HasNewInputSystem) return;

        _logger = logger;
        _devices = devices;
        _updateEvents = updateEvents;

        _updateEvents.OnUpdateActual += UpdateDevices;
        movieRunnerEvents.OnMovieStart += OnMovieStart;
        movieRunnerEvents.OnMovieEnd += OnMovieEnd;
    }

    private void LogConnectedDevices()
    {
        _logger.LogDebug(
            $"Connected devices:\n{InputSystem.devices.Select(x => $"name: {x.name}, type: {x.GetType().FullName}").Join()}");
    }

    private void UpdateDevices()
    {
        if (!_overridden) return;

        foreach (var device in _devices)
        {
            device.Update();
        }
    }

    private void OnMovieStart()
    {
        _overridden = true;

        foreach (var device in _devices)
        {
            device.AddDevice();
            device.MakeCurrent();
        }

        _logger.LogDebug("adding TAS devices to InputSystem");
        LogConnectedDevices();
    }

    private void OnMovieEnd()
    {
        _overridden = false;

        foreach (var device in _devices)
        {
            device.RemoveDevice();
        }

        _logger.LogDebug("removed all TAS devices from InputSystem");
    }
}
