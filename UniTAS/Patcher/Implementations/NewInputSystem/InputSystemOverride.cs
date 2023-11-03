using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.InputSystemOverride;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Services.NewInputSystem;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Services.UnityEvents;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
[ForceInstantiate]
public class InputSystemOverride : IInputSystemOverride, IInputSystemAddedDeviceTracker
{
    private readonly IInputOverrideDevice[] _devices;

    private readonly ILogger _logger;
    private readonly IMovieRunner _movieRunner;
    private readonly bool _hasInputSystem;

    private bool _override;

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public InputSystemOverride(ILogger logger, IInputOverrideDevice[] devices,
        INewInputSystemExists newInputSystemExists, IUpdateEvents updateEvents, IMovieRunner movieRunner,
        IMovieRunnerEvents movieRunnerEvents, IGameRestart gameRestart)
    {
        _hasInputSystem = newInputSystemExists.HasInputSystem;
        logger.LogMessage($"Has unity new input system: {_hasInputSystem}");

        if (!_hasInputSystem) return;

        _logger = logger;
        _devices = devices;
        _movieRunner = movieRunner;
        updateEvents.OnInputUpdateActual += UpdateDevices;
        movieRunnerEvents.OnMovieStart += OnMovieStart;
        movieRunnerEvents.OnMovieEnd += OnMovieEnd;
    }

    public bool Override
    {
        set
        {
            if (!_hasInputSystem) return;
            _override = value;

            if (value)
            {
                _logger.LogDebug("Adding TAS devices to InputSystem");

                foreach (var device in _devices)
                {
                    InputSystem.AddDevice(device.Device);
                }

                _logger.LogDebug("Added TAS devices to InputSystem");
                _logger.LogDebug(
                    $"Connected devices:\n{string.Join("\n", InputSystem.devices.Select(x => $"name: {x.name}, type: {x.GetType().FullName}").ToArray())}");
            }
            else
            {
                _logger.LogDebug($"Removing TAS devices from InputSystem, {new StackTrace()}");

                foreach (var device in _devices)
                {
                    InputSystem.RemoveDevice(device.Device);
                }

                _logger.LogDebug("Removed TAS devices from InputSystem");
            }
        }
    }

    private void UpdateDevices(bool fixedUpdate, bool newInputSystemUpdate)
    {
        if (!newInputSystemUpdate || !_override) return;

        foreach (var device in _devices)
        {
            device.Update();
        }
    }

    public void AddDevice(InputDevice device)
    {
        // if (_movieRunner.MovieEnd) return;
        //
        // _logger.LogDebug($"Adding device on movie end ({device.name} - {device.GetType().Name})");
        // _deviceToAddOnMovieEnd.Add(device);
    }

    private void OnMovieStart()
    {
        Override = true;
    }

    private void OnMovieEnd()
    {
        Override = false;
    }
}