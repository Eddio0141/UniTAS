using UniTAS.Patcher.Models.VirtualEnvironment;

namespace UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

public interface IMouseStateEnvLegacySystem
{
    bool MousePresent { get; }
    bool IsButtonDown(MouseButton button);
    bool IsButtonUp(MouseButton button);
    bool AnyButtonHeld { get; }
    bool AnyButtonDown { get; }
}
