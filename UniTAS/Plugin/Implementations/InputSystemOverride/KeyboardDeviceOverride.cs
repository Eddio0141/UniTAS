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

namespace UniTAS.Plugin.Implementations.InputSystemOverride;

[Singleton]
public class KeyboardDeviceOverride : InputOverrideDevice
{
    private Keyboard _keyboard;

    private readonly IKeyboardStateEnvLegacySystem _keyboardStateEnvLegacySystem;

    public KeyboardDeviceOverride(IVirtualEnvController virtualEnvController, IUpdateEvents updateEvents,
        IInputSystemExists inputSystemExists, IKeyboardStateEnvLegacySystem keyboardStateEnvLegacySystem) : base(
        virtualEnvController,
        updateEvents, inputSystemExists)
    {
        _keyboardStateEnvLegacySystem = keyboardStateEnvLegacySystem;
    }

    [InputControlLayout(stateType = typeof(KeyboardState), isGenericTypeOfDevice = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private class TASKeyboard : Keyboard
    {
    }

    protected override void Update()
    {
    }

    public override void DeviceAdded()
    {
        _keyboard = InputSystem.AddDevice<TASKeyboard>();
    }
}