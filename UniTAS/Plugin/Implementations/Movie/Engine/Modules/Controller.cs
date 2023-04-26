using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Plugin.Interfaces.Movie;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;

namespace UniTAS.Plugin.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class Controller : EngineMethodClass
{
    private readonly IAxisStateEnv _axisStateEnv;
    private readonly IButtonStateEnv _buttonStateEnv;

    [MoonSharpHidden]
    public Controller(IAxisStateEnv axisStateEnv, IButtonStateEnv buttonStateEnv)
    {
        _axisStateEnv = axisStateEnv;
        _buttonStateEnv = buttonStateEnv;
    }

    public void Axis(string axis, float value)
    {
        _axisStateEnv.SetAxis(axis, value);
    }

    public void Hold(string button)
    {
        _buttonStateEnv.Hold(button);
    }

    public void Release(string button)
    {
        _buttonStateEnv.Release(button);
    }

    public void Clear_buttons()
    {
        _buttonStateEnv.Clear();
    }
}