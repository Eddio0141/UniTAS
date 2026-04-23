using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.VirtualEnvironment;
using UniTAS.Patcher.Models.UnitySafeWrappers;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState.NewInputSystem;

[Singleton]
public class MouseState(ICursorWrapper cursorWrapper)
    : LegacyInputSystemButtonBasedDevice<MouseButtonWrap>, IMouseStateEnvLegacySystem, IMouseState
{
    public Vector2 Position { get; set; }
    public Vector2 Delta { get; private set; }
    public Vector2 Scroll { get; set; }

    private Vector2 _prevPos;
    private CursorLockMode _prevLockState;

    protected override void ResetState()
    {
        Position = Vector2.zero;
        Delta = Vector2.zero;
        Scroll = Vector2.zero;
        _prevPos = Vector2.zero;
        _prevLockState = CursorLockMode.None;
    }

    protected override void Update()
    {
        base.Update();

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
    }

    public void HoldButton(MouseButton button)
    {
        Hold(new MouseButtonWrap(button));
    }

    public void ReleaseButton(MouseButton button)
    {
        Release(new MouseButtonWrap(button));
    }

    public bool IsButtonHeld(MouseButton button)
    {
        return IsButtonHeld(new MouseButtonWrap(button));
    }

    public bool IsButtonDown(MouseButton button)
    {
        return IsButtonDown(new MouseButtonWrap(button));
    }

    public bool IsButtonUp(MouseButton button)
    {
        return IsButtonUp(new MouseButtonWrap(button));
    }

    public bool MousePresent => true;

    bool IMouseStateEnvLegacySystem.AnyButtonHeld => AnyButtonHeld;

    bool IMouseStateEnvLegacySystem.AnyButtonDown => AnyButtonDown;
}
