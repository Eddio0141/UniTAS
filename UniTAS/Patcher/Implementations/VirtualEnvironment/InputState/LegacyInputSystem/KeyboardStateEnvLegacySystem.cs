using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.VirtualEnvironment;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState.LegacyInputSystem;

[Singleton]
public class KeyboardStateEnvLegacySystem : LegacyInputSystemButtonBasedDevice<Key>, IKeyboardStateEnvLegacySystem
{
    public new void Hold(Key key)
    {
        base.Hold(key);
    }

    public new void Release(Key key)
    {
        base.Release(key);
    }

    public void Clear()
    {
        ReleaseAllButtons();
    }

    public bool IsKeyDown(Key key)
    {
        return IsButtonDown(key);
    }

    public bool IsKeyUp(Key key)
    {
        return IsButtonUp(key);
    }

    public bool IsKeyHeld(Key key)
    {
        return IsButtonHeld(key);
    }

    public bool AnyKeyHeld => AnyButtonHeld;
    public bool AnyKeyDown => AnyButtonDown;
}