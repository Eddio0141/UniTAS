using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.VirtualEnvironment;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState.LegacyInputSystem;

[Singleton]
public class KeyboardStateEnvLegacySystem : LegacyInputSystemButtonBasedDevice<KeyCodeWrap>,
    IKeyboardStateEnvLegacySystem
{
    public void Hold(KeyCode keyCodeWrap)
    {
        base.Hold(new(keyCodeWrap));
    }

    public void Release(KeyCode keyCodeWrap)
    {
        base.Release(new(keyCodeWrap));
    }

    public void Clear()
    {
        ReleaseAllButtons();
    }

    public bool IsKeyDown(KeyCode keyCodeWrap)
    {
        return IsButtonDown(new(keyCodeWrap));
    }

    public bool IsKeyUp(KeyCode keyCodeWrap)
    {
        return IsButtonUp(new(keyCodeWrap));
    }

    public bool IsKeyHeld(KeyCode keyCodeWrap)
    {
        return IsButtonHeld(new(keyCodeWrap));
    }

    public bool AnyKeyHeld => AnyButtonHeld;
    public bool AnyKeyDown => AnyButtonDown;
}