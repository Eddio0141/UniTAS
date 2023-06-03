using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.InputSystemOverride;
using UniTAS.Plugin.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using MouseButton = UniTAS.Plugin.Models.VirtualEnvironment.MouseButton;

namespace UniTAS.Plugin.Implementations.NewInputSystem;

[Singleton]
public class MouseDeviceOverride : IInputOverrideDevice
{
    private Mouse _mouse;

    private readonly IMouseStateEnvNewSystem _mouseStateEnvNewSystem;

    public MouseDeviceOverride(IMouseStateEnvNewSystem mouseStateEnvNewSystem)
    {
        _mouseStateEnvNewSystem = mouseStateEnvNewSystem;
    }

    [InputControlLayout(stateType = typeof(MouseState), isGenericTypeOfDevice = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private class TASMouse : Mouse
    {
    }

    public void Update()
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

        InputSystem.QueueStateEvent(_mouse, state);
    }

    public void DeviceAdded()
    {
        _mouse = InputSystem.AddDevice<TASMouse>();
    }
}