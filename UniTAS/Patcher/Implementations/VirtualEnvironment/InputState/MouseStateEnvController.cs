using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState;

[Singleton]
public class MouseStateEnvController(
    IMouseStateEnvLegacySystem mouseStateEnvLegacySystem,
    IMouseStateEnvNewSystem mouseStateEnvNewSystem,
    IAxisStateEnvLegacySystem axisStateEnvLegacySystem)
    : IMouseStateEnvController, IOnVirtualEnvStatusChange, IOnGameRestart
{
    public void OnVirtualEnvStatusChange(bool runVirtualEnv)
    {
        if (!runVirtualEnv) return;

        _prevMousePosition = Vector2.zero;
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        _prevMousePosition = Vector2.zero;
    }

    private Vector2 _prevMousePosition;

    public void SetPosition(Vector2 position)
    {
        if (_prevMousePosition == position) return;
        _prevMousePosition = position;

        position = InputSystemUtils.MousePosConstraintInScreen(position);
        mouseStateEnvNewSystem.Position = position;
        mouseStateEnvLegacySystem.Position = position;
        axisStateEnvLegacySystem.MouseMove(position);
    }

    public void SetPositionRelative(Vector2 position)
    {
        if (position is { x: 0, y: 0 }) return;
        _prevMousePosition += position;

        _prevMousePosition = InputSystemUtils.MousePosConstraintInScreen(_prevMousePosition);
        mouseStateEnvNewSystem.Position = _prevMousePosition;
        mouseStateEnvLegacySystem.Position = _prevMousePosition;
        axisStateEnvLegacySystem.MouseMove(_prevMousePosition);
    }

    public void SetScroll(Vector2 scroll)
    {
        mouseStateEnvNewSystem.Scroll = scroll;
        mouseStateEnvLegacySystem.Scroll = scroll;
        axisStateEnvLegacySystem.MouseScroll(scroll.y);
    }

    public void HoldButton(MouseButton button)
    {
        mouseStateEnvNewSystem.HoldButton(button);
        mouseStateEnvLegacySystem.HoldButton(button);
        axisStateEnvLegacySystem.KeyDown(FromMouseButton(button), JoyNum.AllJoysticks);
    }

    public void ReleaseButton(MouseButton button)
    {
        mouseStateEnvNewSystem.ReleaseButton(button);
        mouseStateEnvLegacySystem.ReleaseButton(button);
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