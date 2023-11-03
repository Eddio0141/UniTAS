using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Interfaces.InputSystemOverride;

public interface IInputOverrideDevice
{
    /// <summary>
    /// Update the state of the TAS device
    /// </summary>
    void Update();

    InputDevice Device { get; }
}