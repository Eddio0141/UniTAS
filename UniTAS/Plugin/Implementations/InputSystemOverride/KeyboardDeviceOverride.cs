using System.Diagnostics.CodeAnalysis;
using UniTAS.Plugin.Interfaces.InputSystemOverride;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;

namespace UniTAS.Plugin.Implementations.InputSystemOverride;

public class KeyboardDeviceOverride : InputOverrideDevice
{
    private Keyboard _keyboard;

    private readonly IKeyboardStateEnv _keyboardStateEnv;

    public KeyboardDeviceOverride(IVirtualEnvController virtualEnvController, IKeyboardStateEnv keyboardStateEnv) :
        base(virtualEnvController)
    {
        _keyboardStateEnv = keyboardStateEnv;
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