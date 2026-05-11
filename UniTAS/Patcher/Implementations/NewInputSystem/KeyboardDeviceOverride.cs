using System;
using UniTAS.Patcher.Interfaces.InputSystemOverride;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.NewInputSystem;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

public class KeyboardDeviceOverride(IPatchReverseInvoker reverseInvoker, IKeyboardStateNew keyboardStateEnvNewSystem) : InputOverrideDevice(reverseInvoker)
{
    private readonly IKeyboardStateNew _keyboardStateNew = keyboardStateEnvNewSystem;

    [InputControlLayout(stateType = typeof(KeyboardState), isGenericTypeOfDevice = true)]
    private class TASKeyboard : Keyboard;

    protected override Type InputControlLayout => typeof(TASKeyboard);

    public override void Update()
    {
        var state = new KeyboardState();
        foreach (var heldKey in _keyboardStateNew.HeldKeys)
        {
            state.Set(heldKey.Key, true);
        }

        QueueStateEvent(state);
    }
}
