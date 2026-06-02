using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Models.UnityInfo;
using UniTAS.Patcher.Models.UnitySafeWrappers;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState;

[Singleton(RegisterPriority.MouseState)]
public class MouseState(ICursorWrapper cursorWrapper, IAxisState axisState) : IMouseStateLegacy, IMouseState, IOnVirtualEnvStatusChange, IOnGameRestart, IOnMovieUpdate
{
    private readonly BufferedFullKeyState<MouseButtonWrap> _buttons = new();
    public Vector2 Position { get; set; }
    public Vector2 Delta { get; private set; }
    public Vector2 Scroll
    {
        get => scroll;
        set
        {
            scroll = value;
            axisState.MouseScroll(value.y);
        }
    }

    private Vector2 _prevPos;
    private CursorLockMode _prevLockState;
    private Vector2 scroll;

    private void ResetState()
    {
        Position = Vector2.zero;
        Delta = Vector2.zero;
        Scroll = Vector2.zero;
        _prevPos = Vector2.zero;
        _prevLockState = CursorLockMode.None;
    }

    public void HoldButton(MouseButton button)
    {
        _buttons.Hold(new MouseButtonWrap(button));
        axisState.KeyDown(FromMouseButton(button), JoyNum.AllJoysticks);
    }

    public void ReleaseButton(MouseButton button)
    {
        _buttons.Release(new MouseButtonWrap(button));
        axisState.KeyUp(FromMouseButton(button), JoyNum.AllJoysticks);
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

    public bool IsButtonHeld(MouseButton button)
    {
        return _buttons.IsHeld(new MouseButtonWrap(button));
    }

    public bool IsButtonDown(MouseButton button)
    {
        return _buttons.IsDown(new MouseButtonWrap(button));
    }

    public bool IsButtonUp(MouseButton button)
    {
        return _buttons.IsUp(new MouseButtonWrap(button));
    }

    public void OnVirtualEnvStatusChange(bool runVirtualEnv)
    {
        if (!runVirtualEnv) return;
        ResetState();
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (!preSceneLoad) return;
        ResetState();
    }

    public void MovieUpdate(bool fixedUpdate)
    {
        if (fixedUpdate) return;

        _buttons.Update();

        var position = Position;

        var locked = _prevLockState == CursorLockMode.Locked;
        if (!locked)
        {
            position.x = Mathf.Clamp(position.x, 0, Screen.width);
            position.y = Mathf.Clamp(position.y, 0, Screen.height);
        }

        Delta = position - _prevPos;

        if (locked && position != _prevPos)
        {
            // position is locked to center
            // delta must be calculated before position is locked
            position = new Vector2(Screen.width / 2f, Screen.height / 2f);
        }

        Position = position;
        _prevPos = position;
        _prevLockState = cursorWrapper.LockState;

        axisState.MouseMoveRel(Delta);
    }

    public bool MousePresent => true;

    public bool AnyButtonHeld => _buttons.Held.Count > 0;

    public bool AnyButtonDown => _buttons.Down.Count > 0;
}
