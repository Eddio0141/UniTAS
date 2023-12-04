using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Key : EngineMethodClass
{
    private readonly IKeyboardStateEnvController _kbController;
    private readonly IAxisStateEnvLegacySystem _axisStateEnvLegacySystem;

    [MoonSharpHidden]
    public Key(IKeyboardStateEnvController kbController, IAxisStateEnvLegacySystem axisStateEnvLegacySystem)
    {
        _kbController = kbController;
        _axisStateEnvLegacySystem = axisStateEnvLegacySystem;
    }

    public void Hold(string key)
    {
        _kbController.Hold(key);
        _axisStateEnvLegacySystem.KeyDown(key);
    }

    public void Release(string key)
    {
        _axisStateEnvLegacySystem.KeyUp(key);
        _kbController.Release(key);
    }

    public void Clear()
    {
        _kbController.Clear();
    }
}