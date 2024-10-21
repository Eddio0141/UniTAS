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
}