using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;

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

    public void Hold(Key key)
    {
        _keyboardStateEnvLegacySystem.Hold(key);
        _keyboardStateEnvNewSystem.Hold(key);
    }

    public void Release(Key key)
    {
        _keyboardStateEnvLegacySystem.Release(key);
        _keyboardStateEnvNewSystem.Release(key);
    }

    public void Clear()
    {
        _keyboardStateEnvLegacySystem.Clear();
        _keyboardStateEnvNewSystem.Clear();
    }
}