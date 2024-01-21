using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState;

[Singleton]
public class KeyboardStateEnvController(
    IKeyboardStateEnvLegacySystem keyboardStateEnvLegacySystem,
    IKeyboardStateEnvNewSystem keyboardStateEnvNewSystem)
    : IKeyboardStateEnvController
{
    public void Hold(string key)
    {
        InputSystemUtils.KeyStringToKeys(key, out var keyCode, out var newKey);

        if (keyCode.HasValue)
        {
            keyboardStateEnvLegacySystem.Hold(keyCode.Value);
        }

        if (newKey.HasValue)
        {
            keyboardStateEnvNewSystem.Hold(newKey.Value);
        }
    }

    public void Release(string key)
    {
        InputSystemUtils.KeyStringToKeys(key, out var keyCode, out var newKey);

        if (keyCode.HasValue)
        {
            keyboardStateEnvLegacySystem.Release(keyCode.Value);
        }

        if (newKey.HasValue)
        {
            keyboardStateEnvNewSystem.Release(newKey.Value);
        }
    }

    public void Clear()
    {
        keyboardStateEnvLegacySystem.Clear();
        keyboardStateEnvNewSystem.Clear();
    }
}