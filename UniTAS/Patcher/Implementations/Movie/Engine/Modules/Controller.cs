using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[method: MoonSharpHidden]
public class Controller(IAxisStateEnvLegacySystem axisStateEnvLegacySystem, IMovieRunner movieRunner)
    : EngineMethodClass
{
    private uint _controllerCount;

    // TODO controller max count depends on unity version
    private const uint MAX_CONTROLLER_COUNT = 4;
    private const int BUTTON_MIN_VALUE = 0;
    private const int BUTTON_MAX_VALUE = 19;

    public ControllerInstance Add_player()
    {
        if (_controllerCount >= MAX_CONTROLLER_COUNT)
        {
            movieRunner.MovieLogger.LogError(
                $"Couldn't add another player as it exceeds {MAX_CONTROLLER_COUNT} controllers allowed in this unity version");
            return null;
        }

        _controllerCount += 1;
        return new(axisStateEnvLegacySystem, _controllerCount);
    }

    public void X_axis(float value)
    {
        axisStateEnvLegacySystem.SetAxis(AxisChoice.XAxis, value);
    }

    public void Y_axis(float value)
    {
        axisStateEnvLegacySystem.SetAxis(AxisChoice.YAxis, value);
    }

    public void Axis(int axis, float value)
    {
        // 1..=28 are the only valid values
        const int axisValueMin = 1;
        const int axisValueMax = 28;
        if (axis is < axisValueMin or > axisValueMax)
        {
            movieRunner.MovieLogger.LogWarning(
                $"Supplied axis value is out of range of {axisValueMin} ~ {axisValueMax}, ignoring");
            return;
        }

        axis -= axisValueMin;
        var axisChoice = (AxisChoice)axis;

        axisStateEnvLegacySystem.SetAxis(axisChoice, value);
    }

    public void Hold(int button)
    {
        if (!VerifyHoldButton(button))
        {
            ButtonNumberOutOfRangeWarn();
            return;
        }

        axisStateEnvLegacySystem.KeyDown(GetButtonChoice(button), JoyNum.AllJoysticks);
    }

    public void Release(int button)
    {
        if (!VerifyHoldButton(button))
        {
            ButtonNumberOutOfRangeWarn();
            return;
        }

        axisStateEnvLegacySystem.KeyUp(GetButtonChoice(button), JoyNum.AllJoysticks);
    }

    private void ButtonNumberOutOfRangeWarn()
    {
        movieRunner.MovieLogger.LogWarning(
            $"Button number is not in the range {BUTTON_MIN_VALUE} ~ {BUTTON_MAX_VALUE}");
    }

    private static bool VerifyHoldButton(int button)
    {
        // 0..=19 are the only valid values
        return button is >= BUTTON_MIN_VALUE and <= BUTTON_MAX_VALUE;
    }

    private static string GetButtonChoice(int button)
    {
        return $"joystick button {button}";
    }
}