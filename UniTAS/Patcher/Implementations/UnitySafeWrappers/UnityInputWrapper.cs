using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

[Singleton]
public class UnityInputWrapper(
    IVirtualEnvController venvController,
    IPatchReverseInvoker reverseInvoker,
    IInputSystemState inputSystemState) : IUnityInputWrapper
{
    private readonly bool _useOldInputSystem = inputSystemState.HasOldInputSystem;

    public bool GetKeyDown(KeyCode keyCode, bool reverseInvoke = true)
    {
        if (_useOldInputSystem)
            return reverseInvoke ? reverseInvoker.Invoke(() => Input.GetKeyDown(keyCode)) : Input.GetKeyDown(keyCode);

        // new input system
        if (reverseInvoke && venvController.RunVirtualEnvironment) return false;

        var key = InputSystemUtils.NewKeyParse(keyCode);
        return key != null && Keyboard.current[key.Value].wasPressedThisFrame;
    }

    public Vector2 GetMousePosition(bool reverseInvoke = true)
    {
        if (_useOldInputSystem)
            return reverseInvoke ? reverseInvoker.Invoke(() => Input.mousePosition) : Input.mousePosition;

        // new input system
        if (reverseInvoke && venvController.RunVirtualEnvironment) return new();

        return Mouse.current.position.ReadValue();
    }

    public bool GetAnyKeyDown(bool reverseInvoke = true)
    {
        if (_useOldInputSystem)
            return reverseInvoke ? reverseInvoker.Invoke(() => Input.anyKeyDown) : Input.anyKeyDown;

        if (reverseInvoke && venvController.RunVirtualEnvironment) return false;

        return Keyboard.current.anyKey.wasPressedThisFrame;
    }

    public bool GetMouseButtonDown(int button, bool reverseInvoke = true)
    {
        if (_useOldInputSystem)
            return reverseInvoke
                ? reverseInvoker.Invoke(() => Input.GetMouseButtonDown(button))
                : Input.GetMouseButtonDown(button);

        if (reverseInvoke && venvController.RunVirtualEnvironment) return false;

        return FromMouseButton(button)?.wasPressedThisFrame ?? false;
    }

    public bool GetMouseButton(int button, bool reverseInvoke = true)
    {
        if (_useOldInputSystem)
            return reverseInvoke
                ? reverseInvoker.Invoke(() => Input.GetMouseButton(button))
                : Input.GetMouseButton(button);

        if (reverseInvoke && venvController.RunVirtualEnvironment) return false;

        return FromMouseButton(button)?.isPressed ?? false;
    }

    private static ButtonControl FromMouseButton(int button)
    {
        var mouse = Mouse.current;

        return button switch
        {
            0 => mouse.leftButton,
            1 => mouse.rightButton,
            2 => mouse.middleButton,
            _ => null
        };
    }
}