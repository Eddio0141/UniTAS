using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Services.EventSubscribers;
using UniTAS.Plugin.Services.InputSystemOverride;
using UniTAS.Plugin.Services.VirtualEnvironment;

namespace UniTAS.Plugin.Interfaces.InputSystemOverride;

[RegisterAll]
public abstract class InputOverrideDevice
{
    private readonly IVirtualEnvController _virtualEnvController;

    protected InputOverrideDevice(IVirtualEnvController virtualEnvController, IUpdateEvents updateEvents,
        IInputSystemExists inputSystemExists)
    {
        _virtualEnvController = virtualEnvController;

        if (!inputSystemExists.HasInputSystem) return;
        updateEvents.OnInputUpdateActual += InputUpdateActual;
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