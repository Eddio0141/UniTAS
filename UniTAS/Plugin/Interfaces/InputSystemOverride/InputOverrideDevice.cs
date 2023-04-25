using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.DontRunIfPaused;
using UniTAS.Plugin.Services.VirtualEnvironment;

namespace UniTAS.Plugin.Interfaces.InputSystemOverride;

[RegisterAll]
public abstract class InputOverrideDevice : IOnInputUpdateActual
{
    private readonly IVirtualEnvController _virtualEnvController;

    protected InputOverrideDevice(IVirtualEnvController virtualEnvController)
    {
        _virtualEnvController = virtualEnvController;
    }

    /// <summary>
    /// Update the state of the TAS device
    /// </summary>
    protected abstract void Update();

    /// <summary>
    /// Called when the device is to be added to the input system
    /// </summary>
    public abstract void DeviceAdded();

    public void InputUpdateActual()
    {
        if (!_virtualEnvController.RunVirtualEnvironment) return;

        Update();
    }
}