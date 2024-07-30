using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.InputSystemOverride;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.Logging;
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
    private readonly IVirtualEnvController _virtualEnv;

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public InputSystemOverride(ILogger logger, IInputOverrideDevice[] devices,
        IInputSystemState newInputSystemExists, IUpdateEvents updateEvents, IGameRestart gameRestart, IVirtualEnvController virtualEnv)
    {
        _hasInputSystem = newInputSystemExists.HasNewInputSystem;
        logger.LogMessage($"Has unity new input system: {_hasInputSystem}");

        if (!_hasInputSystem) return;

        _logger = logger;
        _devices = devices;
        _virtualEnv = virtualEnv;
        updateEvents.OnInputUpdateActual += UpdateDevices;

        _logger.LogDebug("Adding TAS devices to InputSystem");

        foreach (var device in _devices)
        {
            device.AddDevice();
        }

        _logger.LogDebug("Added TAS devices to InputSystem");
        _logger.LogDebug(
            $"Connected devices:\n{string.Join("\n", InputSystem.devices.Select(x => $"name: {x.name}, type: {x.GetType().FullName}").ToArray())}");
    }

    private void UpdateDevices(bool fixedUpdate, bool newInputSystemUpdate)
    {
        if (!newInputSystemUpdate || !_virtualEnv.RunVirtualEnvironment) return;

        foreach (var device in _devices)
        {
            device.Update();
        }
    }
}
