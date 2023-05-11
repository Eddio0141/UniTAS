using UniTAS.Plugin.Interfaces.DependencyInjection;

namespace UniTAS.Plugin.Interfaces.InputSystemOverride;

[RegisterAll]
public interface IInputOverrideDevice
{
    /// <summary>
    /// Update the state of the TAS device
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// Called when the device is to be added to the input system
    /// </summary>
    public abstract void DeviceAdded();
}