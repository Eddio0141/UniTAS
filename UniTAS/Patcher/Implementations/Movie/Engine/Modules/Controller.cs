using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Models.UnityInfo;
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

    public void X_axis(float value)
    {
        _axisStateEnvLegacySystem.SetAxis(AxisChoice.XAxis, value);
    }

    public void Y_axis(float value)
    {
        _axisStateEnvLegacySystem.SetAxis(AxisChoice.YAxis, value);
    }

    public void Axis(int axis, float value)
    {
        // 1..=28 are the only valid values
        if (axis is < 1 or > 28)
        {
            return;
        }

        axis -= 1;
        var axisChoice = (AxisChoice)axis;

        _axisStateEnvLegacySystem.SetAxis(axisChoice, value);
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