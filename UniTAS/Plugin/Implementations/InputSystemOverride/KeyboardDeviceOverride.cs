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

namespace UniTAS.Plugin.Implementations.InputSystemOverride;

[Singleton]
public class KeyboardDeviceOverride : InputOverrideDevice
{
    private Keyboard _keyboard;

    private readonly IKeyboardStateEnvNewSystem _keyboardStateEnvNewSystem;

    public KeyboardDeviceOverride(IVirtualEnvController virtualEnvController, IUpdateEvents updateEvents,
        IInputSystemExists inputSystemExists, IKeyboardStateEnvNewSystem keyboardStateEnvNewSystem) : base(
        virtualEnvController, updateEvents, inputSystemExists)
    {
        _keyboardStateEnvNewSystem = keyboardStateEnvNewSystem;
    }

    [InputControlLayout(stateType = typeof(KeyboardState), isGenericTypeOfDevice = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private class TASKeyboard : Keyboard
    {
    }

    protected override void Update()
    {
        var state = new KeyboardState();
        foreach (var heldKey in _keyboardStateEnvNewSystem.HeldKeys)
        {
            var heldKeyNewSystem = heldKey.NewInputSystemKey;
            if (heldKeyNewSystem == null) continue;
            state.Set(heldKeyNewSystem.Value, true);
        }

        InputSystem.QueueStateEvent(_keyboard, state);
    }

    public override void DeviceAdded()
    {
        _keyboard = InputSystem.AddDevice<TASKeyboard>();
    }
}