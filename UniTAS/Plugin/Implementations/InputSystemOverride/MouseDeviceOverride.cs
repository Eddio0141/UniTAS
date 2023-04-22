using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.InputSystemOverride;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;

namespace UniTAS.Plugin.Implementations.InputSystemOverride;

[Singleton]
public class MouseDeviceOverride : InputOverrideDevice
{
    private Mouse _mouse;

    private readonly IMouseStateEnv _mouseStateEnv;

    [InputControlLayout(stateType = typeof(MouseState), isGenericTypeOfDevice = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private class TASMouse : Mouse
    {
    }

    public MouseDeviceOverride(IVirtualEnvController virtualEnvController, IMouseStateEnv mouseStateEnv) : base(
        virtualEnvController)
    {
        _mouseStateEnv = mouseStateEnv;
    }

    protected override void Update()
    {
        var state = new MouseState
        {
            // bit 0 = left mouse button
            buttons = (ushort)(_mouseStateEnv.LeftClick ? 1 : 0),
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