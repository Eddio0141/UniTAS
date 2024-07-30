using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Interfaces.InputSystemOverride;

public interface IInputOverrideDevice
{
    InputDevice Device { get; }

    /// <summary>
    /// Update the state of the TAS device
    /// </summary>
    void Update();

    /// <summary>
    /// Called when the device is to be added to the input system
    /// </summary>
    void AddDevice();
}
