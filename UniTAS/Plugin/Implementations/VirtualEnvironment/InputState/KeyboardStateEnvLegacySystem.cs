using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.VirtualEnvironment;
using UniTAS.Plugin.Models.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input.LegacyInputSystem;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment.InputState;

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