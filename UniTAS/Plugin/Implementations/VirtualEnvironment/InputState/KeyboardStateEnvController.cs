using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Models.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UniTAS.Plugin.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Plugin.Services.VirtualEnvironment.Input.NewInputSystem;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment.InputState;

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