using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState;

[Singleton]
public class MouseStateEnvController(IMouseState mouseState, IAxisStateEnvLegacySystem axisStateEnvLegacySystem) : IMouseStateEnvController
{
    public void SetPosition(Vector2 position)
    {
        mouseState.Position = position;
        axisStateEnvLegacySystem.MouseMoveRel(mouseState.Delta);
    }

    public void SetPositionRelative(Vector2 position)
    {
        mouseState.Position += position;
        axisStateEnvLegacySystem.MouseMoveRel(mouseState.Position);
    }

    public void SetScroll(Vector2 scroll)
    {
        mouseState.Scroll = scroll;
        axisStateEnvLegacySystem.MouseScroll(scroll.y);
    }

    public void HoldButton(MouseButton button)
    {
        mouseState.HoldButton(button);
        axisStateEnvLegacySystem.KeyDown(FromMouseButton(button), JoyNum.AllJoysticks);
    }

    public void ReleaseButton(MouseButton button)
    {
        mouseState.ReleaseButton(button);
        axisStateEnvLegacySystem.KeyUp(FromMouseButton(button), JoyNum.AllJoysticks);
    }

    private static KeyCode FromMouseButton(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => KeyCode.Mouse0,
            MouseButton.Right => KeyCode.Mouse1,
            MouseButton.Middle => KeyCode.Mouse2,
            _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
        };
    }
}
