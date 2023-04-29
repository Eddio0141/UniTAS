using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.InputSystemOverride;
using UniTAS.Plugin.Services.EventSubscribers;
using UniTAS.Plugin.Services.InputSystemOverride;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using MouseButton = UniTAS.Plugin.Models.VirtualEnvironment.MouseButton;

namespace UniTAS.Plugin.Implementations.InputSystemOverride;

[Singleton]
public class MouseDeviceOverride : InputOverrideDevice
{
    private Mouse _mouse;

    private readonly IMouseStateEnvLegacySystem _mouseStateEnvLegacySystem;

    public MouseDeviceOverride(IVirtualEnvController virtualEnvController, IUpdateEvents updateEvents,
        IInputSystemExists inputSystemExists, IMouseStateEnvLegacySystem mouseStateEnvLegacySystem) : base(
        virtualEnvController, updateEvents,
        inputSystemExists)
    {
        _mouseStateEnvLegacySystem = mouseStateEnvLegacySystem;
    }

    [InputControlLayout(stateType = typeof(MouseState), isGenericTypeOfDevice = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private class TASMouse : Mouse
    {
    }

    protected override void Update()
    {
        ushort buttons = 0;
        if (_mouseStateEnvLegacySystem.IsButtonHeld(MouseButton.Left))
        {
            Plugin.Log.LogDebug("Left button held");
            buttons |= 0b1;
        }

        if (_mouseStateEnvLegacySystem.IsButtonHeld(MouseButton.Right))
        {
            Plugin.Log.LogDebug("Right button held");
            buttons |= 0b10;
        }

        if (_mouseStateEnvLegacySystem.IsButtonHeld(MouseButton.Middle))
        {
            Plugin.Log.LogDebug("Middle button held");
            buttons |= 0b100;
        }

        var state = new MouseState
        {
            buttons = buttons,
            position = _mouseStateEnvLegacySystem.Position,
            scroll = _mouseStateEnvLegacySystem.Scroll
        };

        InputSystem.QueueStateEvent(_mouse, state);
    }

    public override void DeviceAdded()
    {
        _mouse = InputSystem.AddDevice<TASMouse>();
    }
}