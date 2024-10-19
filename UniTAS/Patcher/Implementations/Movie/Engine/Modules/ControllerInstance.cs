using System;
using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class ControllerInstance
{
    private readonly IAxisStateEnvLegacySystem _axisStateEnvLegacySystem;

    private readonly uint _playerNumber;
    private readonly JoyNum _playerJoyNum;

    [MoonSharpHidden]
    public ControllerInstance(IAxisStateEnvLegacySystem axisStateEnvLegacySystem, uint playerNumber)
    {
        _axisStateEnvLegacySystem = axisStateEnvLegacySystem;
        _playerNumber = playerNumber;
        _playerJoyNum = playerNumber switch
        {
            1 => JoyNum.Joystick1,
            2 => JoyNum.Joystick2,
            3 => JoyNum.Joystick3,
            4 => JoyNum.Joystick4,
            _ => throw new InvalidOperationException()
        };
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

        _axisStateEnvLegacySystem.KeyDown(GetButtonChoice(button), _playerJoyNum);
    }

    public void Release(int button)
    {
        if (!VerifyHoldButton(button))
        {
            return;
        }

        _axisStateEnvLegacySystem.KeyUp(GetButtonChoice(button), _playerJoyNum);
    }

    private static bool VerifyHoldButton(int button)
    {
        // 0..=19 are the only valid values
        return button is >= 0 and <= 19;
    }

    private KeyCode GetButtonChoice(int button)
    {
        var playerNumberBase = _playerNumber switch
        {
            1 => KeyCode.Joystick1Button0,
            2 => KeyCode.Joystick2Button0,
            3 => KeyCode.Joystick3Button0,
            _ => throw new InvalidOperationException()
        };

        return playerNumberBase + button;
    }
}