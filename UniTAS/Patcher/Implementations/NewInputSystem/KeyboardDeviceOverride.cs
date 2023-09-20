using System.Diagnostics.CodeAnalysis;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.InputSystemOverride;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
public class KeyboardDeviceOverride : IInputOverrideDevice
{
    private Keyboard _keyboard;

    private readonly IKeyboardStateEnvNewSystem _keyboardStateEnvNewSystem;

    public KeyboardDeviceOverride(IKeyboardStateEnvNewSystem keyboardStateEnvNewSystem)
    {
        _keyboardStateEnvNewSystem = keyboardStateEnvNewSystem;
    }

    [InputControlLayout(stateType = typeof(KeyboardState), isGenericTypeOfDevice = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private class TASKeyboard : Keyboard
    {
    }

    public void Update()
    {
        var state = new KeyboardState();
        foreach (var heldKey in _keyboardStateEnvNewSystem.HeldKeys)
        {
            state.Set(heldKey.Key, true);
        }

        InputSystem.QueueStateEvent(_keyboard, state);
    }

    public void DeviceAdded()
    {
        _keyboard = InputSystem.AddDevice<TASKeyboard>();
    }
}