using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UniTAS.Patcher.Utils;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState;

[Singleton]
public class KeyboardStateEnvController : IKeyboardStateEnvController
{
    private readonly IKeyboardStateEnvLegacySystem _keyboardStateEnvLegacySystem;
    private readonly IKeyboardStateEnvNewSystem _keyboardStateEnvNewSystem;

    public KeyboardStateEnvController(IKeyboardStateEnvLegacySystem keyboardStateEnvLegacySystem,
        IKeyboardStateEnvNewSystem keyboardStateEnvNewSystem)
    {
        _keyboardStateEnvLegacySystem = keyboardStateEnvLegacySystem;
        _keyboardStateEnvNewSystem = keyboardStateEnvNewSystem;
    }

    public void Hold(string key)
    {
        InputSystemUtils.KeyStringToKeys(key, out var keyCode, out var newKey);

        if (keyCode.HasValue)
        {
            _keyboardStateEnvLegacySystem.Hold(keyCode.Value);
        }

        if (newKey.HasValue)
        {
            _keyboardStateEnvNewSystem.Hold(newKey.Value);
        }
    }

    public void Release(string key)
    {
        InputSystemUtils.KeyStringToKeys(key, out var keyCode, out var newKey);

        if (keyCode.HasValue)
        {
            _keyboardStateEnvLegacySystem.Release(keyCode.Value);
        }

        if (newKey.HasValue)
        {
            _keyboardStateEnvNewSystem.Release(newKey.Value);
        }
    }

    public void Clear()
    {
        _keyboardStateEnvLegacySystem.Clear();
        _keyboardStateEnvNewSystem.Clear();
    }
}