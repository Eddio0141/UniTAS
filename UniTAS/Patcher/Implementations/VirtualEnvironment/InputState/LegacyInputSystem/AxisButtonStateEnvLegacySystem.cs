using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState.LegacyInputSystem;

[Singleton]
public class AxisButtonStateEnvLegacySystem : LegacyInputSystemButtonBasedDevice<string>,
    IAxisButtonStateEnvLegacySystem, IAxisButtonStateEnvUpdate
{
    public new void Hold(string button)
    {
        base.Hold(button);
    }

    public new void Release(string button)
    {
        base.Release(button);
    }

    public new bool IsButtonHeld(string button)
    {
        return base.IsButtonHeld(button);
    }

    public new bool IsButtonDown(string button)
    {
        return base.IsButtonDown(button);
    }

    public new bool IsButtonUp(string button)
    {
        return base.IsButtonUp(button);
    }
}