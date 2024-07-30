using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.InputSystemOverride;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using MouseButton = UniTAS.Patcher.Models.VirtualEnvironment.MouseButton;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
public class MouseDeviceOverride : IInputOverrideDevice
{
    private readonly IMouseStateEnvNewSystem _mouseStateEnvNewSystem;

    public MouseDeviceOverride(IMouseStateEnvNewSystem mouseStateEnvNewSystem)
    {
        _mouseStateEnvNewSystem = mouseStateEnvNewSystem;
    }

    private TASMouse _device;
    public InputDevice Device => _device;

    [InputControlLayout(stateType = typeof(MouseState), isGenericTypeOfDevice = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private class TASMouse : Mouse;

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

        InputSystem.QueueStateEvent(_device, state);
    }

    public void AddDevice()
    {
        _device = InputSystem.AddDevice<TASMouse>();
    }
}
