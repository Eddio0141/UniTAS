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
    private readonly IKeyboardStateEnvNewSystem _keyboardStateEnvNewSystem;
    private TASKeyboard _device;

    public KeyboardDeviceOverride(IKeyboardStateEnvNewSystem keyboardStateEnvNewSystem)
    {
        _keyboardStateEnvNewSystem = keyboardStateEnvNewSystem;
    }

    [InputControlLayout(stateType = typeof(KeyboardState), isGenericTypeOfDevice = true)]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    private class TASKeyboard : Keyboard;

    public void Update()
    {
        var state = new KeyboardState();
        foreach (var heldKey in _keyboardStateEnvNewSystem.HeldKeys)
        {
            state.Set(heldKey.Key, true);
        }

        InputSystem.QueueStateEvent(_device, state);
    }

    public void AddDevice()
    {
        _device = InputSystem.AddDevice<TASKeyboard>();
    }

    public void RemoveDevice()
    {
        InputSystem.RemoveDevice(_device);
        _device = null;
    }
}