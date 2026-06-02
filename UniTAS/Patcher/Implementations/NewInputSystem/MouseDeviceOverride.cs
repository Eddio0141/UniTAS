using System;
using UniTAS.Patcher.Interfaces.InputSystemOverride;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using MouseButton = UniTAS.Patcher.Models.VirtualEnvironment.MouseButton;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

public class MouseDeviceOverride(IMouseState mouseStateEnvNewSystem) : InputOverrideDevice
{
    private readonly IMouseState _mouseState = mouseStateEnvNewSystem;

    [InputControlLayout(stateType = typeof(MouseState), isGenericTypeOfDevice = true)]
    private class TASMouse : Mouse;

    protected override Type InputControlLayout => typeof(TASMouse);

    public override void Update()
    {
        ushort buttons = 0;
        if (_mouseState.IsButtonHeld(MouseButton.Left))
        {
            buttons |= 0b1;
        }

        if (_mouseState.IsButtonHeld(MouseButton.Right))
        {
            buttons |= 0b10;
        }

        if (_mouseState.IsButtonHeld(MouseButton.Middle))
        {
            buttons |= 0b100;
        }

        var state = new MouseState
        {
            buttons = buttons,
            position = _mouseState.Position,
            delta = _mouseState.Delta,
            scroll = _mouseState.Scroll,
            // TODO: testing with normal game shows its 0 regardless of how fast I click, test more to determine whats best
            // clickCount = ??,
            // displayIndex = ??, // TODO: probably look into it once virtual env gets more enriched with os stuff
        };

        QueueStateEvent(state);
    }
}
