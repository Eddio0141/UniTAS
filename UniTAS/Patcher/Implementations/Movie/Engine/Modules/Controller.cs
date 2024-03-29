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

    private uint _controllerCount;

    // TODO controller max count depends on unity version
    private const uint MAX_CONTROLLER_COUNT = 4;

    [MoonSharpHidden]
    public Controller(IAxisStateEnvLegacySystem axisStateEnvLegacySystem)
    {
        _axisStateEnvLegacySystem = axisStateEnvLegacySystem;
    }

    public ControllerInstance Add_player()
    {
        if (_controllerCount >= MAX_CONTROLLER_COUNT)
        {
            return null;
        }

        _controllerCount += 1;
        return new(_axisStateEnvLegacySystem, _controllerCount);
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

    public void Hold(int button)
    {
        if (!VerifyHoldButton(button))
        {
            return;
        }

        _axisStateEnvLegacySystem.KeyDown(GetButtonChoice(button), JoyNum.AllJoysticks);
    }

    public void Release(int button)
    {
        if (!VerifyHoldButton(button))
        {
            return;
        }

        _axisStateEnvLegacySystem.KeyUp(GetButtonChoice(button), JoyNum.AllJoysticks);
    }

    private static bool VerifyHoldButton(int button)
    {
        // 0..=19 are the only valid values
        return button is >= 0 and <= 19;
    }

    private static string GetButtonChoice(int button)
    {
        return $"joystick button {button}";
    }
}