using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.Implementations.UnitySafeWrappers;

[Singleton]
public class UnityInputWrapper(
    IVirtualEnvController venvController,
    IPatchReverseInvoker reverseInvoker,
    IInputSystemState inputSystemState) : IUnityInputWrapper
{
    private readonly bool _useOldInputSystem = inputSystemState.HasOldInputSystem;

    public bool GetKeyDown(KeyCode keyCode)
    {
        if (_useOldInputSystem)
            return reverseInvoker.Invoke(() => Input.GetKeyDown(keyCode));

        // new input system
        if (venvController.RunVirtualEnvironment) return false;

        var key = InputSystemUtils.NewKeyParse(keyCode);
        return key != null && Keyboard.current[key.Value].wasPressedThisFrame;
    }

    public Vector2 MousePosition
    {
        get
        {
            if (_useOldInputSystem)
                return reverseInvoker.Invoke(() => Input.mousePosition);

            // new input system
            if (venvController.RunVirtualEnvironment) return new();

            return Mouse.current.position.ReadValue();
        }
    }

    public bool AnyKeyDown
    {
        get
        {
            if (_useOldInputSystem)
                return reverseInvoker.Invoke(() => Input.anyKeyDown);

            if (venvController.RunVirtualEnvironment) return false;

            return Keyboard.current.anyKey.wasPressedThisFrame;
        }
    }

    public bool GetMouseButtonDown(int button)
    {
        if (_useOldInputSystem)
            return reverseInvoker.Invoke(() => Input.GetMouseButtonDown(button));

        if (venvController.RunVirtualEnvironment) return false;

        var mouse = Mouse.current;
        return button switch
        {
            0 => mouse.leftButton.wasPressedThisFrame,
            1 => mouse.rightButton.wasPressedThisFrame,
            2 => mouse.middleButton.wasPressedThisFrame,
            _ => false
        };
    }
}