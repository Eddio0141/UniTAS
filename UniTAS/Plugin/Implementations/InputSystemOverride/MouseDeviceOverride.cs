using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.InputSystemOverride;
using UniTAS.Plugin.Services.EventSubscribers;
using UniTAS.Plugin.Services.InputSystemOverride;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using MouseButton = UniTAS.Plugin.Models.VirtualEnvironment.MouseButton;

namespace UniTAS.Plugin.Implementations.InputSystemOverride;

[Singleton]
public class MouseDeviceOverride : InputOverrideDevice
{
    private Mouse _mouse;

    private readonly IMouseStateEnvNewSystem _mouseStateEnvNewSystem;

    public MouseDeviceOverride(IVirtualEnvController virtualEnvController, IUpdateEvents updateEvents,
        IInputSystemExists inputSystemExists, IMouseStateEnvNewSystem mouseStateEnvNewSystem) : base(
        virtualEnvController, updateEvents, inputSystemExists)
    {
        _mouseStateEnvNewSystem = mouseStateEnvNewSystem;
    }

    [InputControlLayout(stateType = typeof(MouseState), isGenericTypeOfDevice = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private class TASMouse : Mouse
    {
    }

    protected override void Update()
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

    public override void DeviceAdded()
    {
        _mouse = InputSystem.AddDevice<TASMouse>();
    }
}