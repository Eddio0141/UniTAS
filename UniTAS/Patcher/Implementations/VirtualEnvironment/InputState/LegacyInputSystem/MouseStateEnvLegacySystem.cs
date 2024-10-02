using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.VirtualEnvironment;
using UniTAS.Patcher.Models.UnitySafeWrappers;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState.LegacyInputSystem;

[Singleton]
[ExcludeRegisterIfTesting]
public class MouseStateEnvLegacySystem(ICursorWrapper cursorWrapper)
    : LegacyInputSystemButtonBasedDevice<MouseButtonWrap>, IMouseStateEnvLegacySystem
{
    public bool MousePresent => true;
    public Vector2 Position { get; set; }
    private Vector2 _prevPosition;
    public Vector2 Scroll { get; set; }

    protected override void ResetState()
    {
        base.ResetState();
        Position = Vector2.zero;
        _prevPosition = Vector2.zero;
        Scroll = Vector2.zero;
    }

    protected override void Update()
    {
        base.Update();

        // simulate cursor warping in different cursor lock state
        // note: i've seen the mouse cursor being able to stay at a non-center coordinate, which is why this operation is done first
        if (_prevPosition != Position && cursorWrapper.LockState == CursorLockMode.Locked)
        {
            Position = new(Screen.width / 2f, Screen.height / 2f);
        }

        _prevPosition = Position;
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

    public void HoldButton(MouseButton button)
    {
        Hold(new(button));
    }

    public void ReleaseButton(MouseButton button)
    {
        Release(new(button));
    }

    public new bool AnyButtonHeld => base.AnyButtonHeld;
    public new bool AnyButtonDown => base.AnyButtonDown;
}