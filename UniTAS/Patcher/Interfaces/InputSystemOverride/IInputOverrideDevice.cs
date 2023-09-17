using UniTAS.Patcher.Interfaces.DependencyInjection;

namespace UniTAS.Patcher.Interfaces.InputSystemOverride;

[RegisterAll]
public interface IInputOverrideDevice
{
    /// <summary>
    /// Update the state of the TAS device
    /// </summary>
    void Update();

    /// <summary>
    /// Called when the device is to be added to the input system
    /// </summary>
    void DeviceAdded();
}