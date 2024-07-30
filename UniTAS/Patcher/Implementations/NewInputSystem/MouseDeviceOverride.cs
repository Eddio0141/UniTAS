using System;
using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.InputSystemOverride;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using MouseButton = UniTAS.Patcher.Models.VirtualEnvironment.MouseButton;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

public class MouseDeviceOverride : InputOverrideDevice
{
    private readonly IMouseStateEnvNewSystem _mouseStateEnvNewSystem;

    public MouseDeviceOverride(IMouseStateEnvNewSystem mouseStateEnvNewSystem)
    {
        _mouseStateEnvNewSystem = mouseStateEnvNewSystem;
    }

    [InputControlLayout(stateType = typeof(MouseState), isGenericTypeOfDevice = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private class TASMouse : Mouse;

    protected override Type InputControlLayout => typeof(TASMouse);

    public override void Update()
    {
        ushort buttons = 0;
        if (_mouseStateEnvNewSystem.IsButtonHeld(MouseButton.Left))
        {
            buttons |= 0b1;
        }

        if (_mouseStateEnvNewSystem.IsButtonHeld(MouseButton.Right))
        {
            buttons |= 0b10;
        }

        if (_mouseStateEnvNewSystem.IsButtonHeld(MouseButton.Middle))
        {
            buttons |= 0b100;
        }

        var state = new MouseState
        {
            buttons = buttons,
            position = _mouseStateEnvNewSystem.Position,
            scroll = _mouseStateEnvNewSystem.Scroll
        };

        InputSystem.QueueStateEvent(Device, state);
    }
}
