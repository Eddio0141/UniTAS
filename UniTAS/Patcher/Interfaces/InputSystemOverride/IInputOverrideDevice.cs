namespace UniTAS.Patcher.Interfaces.InputSystemOverride;

public interface IInputOverrideDevice
{
    /// <summary>
    /// Update the state of the TAS device
    /// </summary>
    void Update();

    /// <summary>
    /// Called when the device is to be added to the input system
    /// </summary>
    void AddDevice();

    /// <summary>
    /// Called when device is to be removed from the input system
    /// </summary>
    void RemoveDevice();
}