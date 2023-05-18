using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input.LegacyInputSystem;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment.InputState.LegacyInputSystem;

[Singleton]
public class ButtonStateEnvLegacySystem : LegacyInputSystemButtonBasedDevice<string>, IButtonStateEnvLegacySystem
{
    public new void Hold(string button)
    {
        base.Hold(button);
    }

    public new void Release(string button)
    {
        base.Release(button);
    }

    public void Clear()
    {
        ReleaseAllButtons();
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