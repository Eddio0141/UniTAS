using System;
using System.Diagnostics.CodeAnalysis;
using MoonSharp.Interpreter;
using UniTAS.Patcher.Interfaces.Movie;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.Movie.Engine.Modules;

[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[method: MoonSharpHidden]
public class Mouse(IMouseStateEnvController mouseController, IAxisStateEnvLegacySystem axisStateEnvLegacySystem)
    : EngineMethodClass
{
    public void Move(float x, float y)
    {
        var mousePos = new Vector2(x, y);
        mouseController.SetPosition(mousePos);
        axisStateEnvLegacySystem.MouseMove(mousePos);
    }

    public void Move_rel(float x, float y)
    {
        var mousePos = new Vector2(x, y);
        mouseController.SetPositionRelative(mousePos);
        axisStateEnvLegacySystem.MouseMoveRelative(mousePos);
    }

    public void Left(bool hold = true)
    {
        HandlePress(hold, MouseButton.Left);
    }

    public void Right(bool hold = true)
    {
        HandlePress(hold, MouseButton.Right);
    }

    public void Middle(bool hold = true)
    {
        HandlePress(hold, MouseButton.Middle);
    }

    private void HandlePress(bool hold, MouseButton button)
    {
        var buttonChoice = button switch
        {
            MouseButton.Left => KeyCode.Mouse0,
            MouseButton.Right => KeyCode.Mouse1,
            MouseButton.Middle => KeyCode.Mouse2,
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };

        if (hold)
        {
            mouseController.HoldButton(button);
            axisStateEnvLegacySystem.KeyDown(buttonChoice, JoyNum.AllJoysticks);
        }
        else
        {
            mouseController.ReleaseButton(button);
            axisStateEnvLegacySystem.KeyUp(buttonChoice, JoyNum.AllJoysticks);
        }
    }

    public void Set_scroll(float x, float y)
    {
        mouseController.SetScroll(new(x, y));
        axisStateEnvLegacySystem.MouseScroll(y);
    }
}