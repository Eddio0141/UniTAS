using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Controller : EngineMethodClass
{
    private readonly IAxisStateEnvLegacySystem _axisStateEnvLegacySystem;
    private readonly IButtonStateEnvLegacySystem _buttonStateEnvLegacySystem;

    [MoonSharpHidden]
    public Controller(IAxisStateEnvLegacySystem axisStateEnvLegacySystem,
        IButtonStateEnvLegacySystem buttonStateEnvLegacySystem)
    {
        _axisStateEnvLegacySystem = axisStateEnvLegacySystem;
        _buttonStateEnvLegacySystem = buttonStateEnvLegacySystem;
    }

    public void Axis(string axis, float value)
    {
        _axisStateEnvLegacySystem.SetAxis(axis, value);
    }

    public void Hold(string button)
    {
        _buttonStateEnvLegacySystem.Hold(button);
    }

    public void Release(string button)
    {
        _buttonStateEnvLegacySystem.Release(button);
    }

    public void Clear_buttons()
    {
        _buttonStateEnvLegacySystem.Clear();
    }
}