using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.InputSystemOverride;
using UniTAS.Plugin.Services.EventSubscribers;
using UniTAS.Plugin.Services.InputSystemOverride;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using MouseButton = UniTAS.Plugin.Models.VirtualEnvironment.MouseButton;

namespace UniTAS.Plugin.Implementations.InputSystemOverride;

[Singleton]
public class MouseDeviceOverride : InputOverrideDevice
{
    private Mouse _mouse;

    private readonly IMouseStateEnv _mouseStateEnv;

    public MouseDeviceOverride(IVirtualEnvController virtualEnvController, IUpdateEvents updateEvents,
        IInputSystemExists inputSystemExists, IMouseStateEnv mouseStateEnv) : base(virtualEnvController, updateEvents,
        inputSystemExists)
    {
        _mouseStateEnv = mouseStateEnv;
    }

    [InputControlLayout(stateType = typeof(MouseState), isGenericTypeOfDevice = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private class TASMouse : Mouse
    {
    }

    protected override void Update()
    {
        ushort buttons = 0;
        if (_mouseStateEnv.IsButtonHeld(MouseButton.Left))
        {
            buttons |= 0b1;
        }

        if (_mouseStateEnv.IsButtonHeld(MouseButton.Right))
        {
            buttons |= 0b10;
        }

        if (_mouseStateEnv.IsButtonHeld(MouseButton.Middle))
        {
            buttons |= 0b100;
        }

        var state = new MouseState
        {
            buttons = buttons,
            position = _mouseStateEnv.Position,
            scroll = _mouseStateEnv.Scroll
        };

        InputSystem.QueueStateEvent(_mouse, state);
    }

    public override void DeviceAdded()
    {
        _mouse = InputSystem.AddDevice<TASMouse>();
    }
}