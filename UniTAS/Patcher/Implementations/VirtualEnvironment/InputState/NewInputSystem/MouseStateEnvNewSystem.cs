using System;
using System.Collections.Generic;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Models.UnitySafeWrappers;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment.InputState.NewInputSystem;

[Singleton]
public class MouseStateEnvNewSystem(ICursorWrapper cursorWrapper) : Interfaces.VirtualEnvironment.InputState, IMouseStateEnvNewSystem
{
    public Vector2 Position { get; set; }
    public Vector2 Delta { get; private set; }
    public Vector2 Scroll { get; set; }
    private Vector2 _prevPos;
    private readonly List<MouseButton> _heldButtons = new();

    protected override void ResetState()
    {
        Position = Vector2.zero;
        _prevPos = Vector2.zero;
        Delta = Vector2.zero;
        Scroll = Vector2.zero;
    }

    protected override void Update()
    {
        base.Update();

        Delta = Position - _prevPos;
        _prevPos = Position;

        // simulate cursor warping in different cursor lock state
        // note: i've seen the mouse cursor being able to stay at a non-center coordinate, which is why this operation is done first
        if (Delta != Vector2.zero && cursorWrapper.LockState == CursorLockMode.Locked)
        {
            // div by float, decimal point is possible in mouse cursor pos
            var center = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Position = center;
            _prevPos = center;

            var warpAmount = center - Position;
            var warpAmountAbs = new Vector2(Math.Abs(warpAmount.x), Math.Abs(warpAmount.y));
            var deltaAbs = new Vector2(Math.Abs(Delta.x), Math.Abs(Delta.y));
            if (warpAmountAbs.x <= deltaAbs.x && warpAmountAbs.y <= deltaAbs.y) return;

            Delta += warpAmount;
        }
    }

    public void HoldButton(MouseButton button)
    {
        if (_heldButtons.Contains(button)) return;
        _heldButtons.Add(button);
    }

    public void ReleaseButton(MouseButton button)
    {
        _heldButtons.Remove(button);
    }

    public bool IsButtonHeld(MouseButton button)
    {
        return _heldButtons.Contains(button);
    }
}
