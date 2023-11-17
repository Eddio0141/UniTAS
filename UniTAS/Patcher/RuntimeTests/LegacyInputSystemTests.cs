using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.RuntimeTest;
using UniTAS.Patcher.Interfaces.VirtualEnvironment;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.RuntimeTests;

[Register]
public class LegacyInputSystemTests
{
    private readonly IKeyboardStateEnvLegacySystem _keyboardController;
    private readonly LegacyInputSystemDevice _keyboardControllerBase;

    private readonly IMouseStateEnvLegacySystem _mouseController;
    private readonly LegacyInputSystemDevice _mouseControllerBase;

    private readonly IVirtualEnvController _virtualEnvController;

    public LegacyInputSystemTests(IKeyboardStateEnvLegacySystem keyboardController,
        IVirtualEnvController virtualEnvController, IMouseStateEnvLegacySystem mouseController)
    {
        _keyboardController = keyboardController;
        _keyboardControllerBase = (LegacyInputSystemDevice)keyboardController;
        _virtualEnvController = virtualEnvController;
        _mouseController = mouseController;
        _mouseControllerBase = (LegacyInputSystemDevice)mouseController;
    }

    [RuntimeTest]
    public void GetKeyTest()
    {
        _virtualEnvController.RunVirtualEnvironment = true;

        _keyboardController.Hold(KeyCode.A);
        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.True(Input.GetKeyDown(KeyCode.A), "keycode down check");
        RuntimeAssert.True(Input.GetKeyDown("a"), "string down check");
        RuntimeAssert.True(Input.GetKey(KeyCode.A), "keycode check");
        RuntimeAssert.True(Input.GetKey("a"), "string check");

        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetKeyDown(KeyCode.A), "keycode down check");
        RuntimeAssert.False(Input.GetKeyDown("a"), "string down check");
        RuntimeAssert.True(Input.GetKey(KeyCode.A), "keycode 2 check");
        RuntimeAssert.True(Input.GetKey("a"), "string 2 check");

        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.True(Input.GetKey(KeyCode.A), "keycode 3 check");
        RuntimeAssert.True(Input.GetKey("a"), "string 3 check");

        _keyboardController.Release(KeyCode.A);
        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetKey(KeyCode.A), "keycode 4 check");
        RuntimeAssert.False(Input.GetKey("a"), "string 4 check");
        RuntimeAssert.True(Input.GetKeyUp(KeyCode.A), "keycode up check");
        RuntimeAssert.True(Input.GetKeyUp("a"), "string up check");

        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetKeyUp(KeyCode.A), "keycode up check");
        RuntimeAssert.False(Input.GetKeyUp("a"), "string up check");

        _virtualEnvController.RunVirtualEnvironment = false;
    }

    [RuntimeTest]
    public void KeyDownTest()
    {
        _virtualEnvController.RunVirtualEnvironment = true;

        _keyboardController.Hold(KeyCode.A);
        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.True(Input.GetKeyDown(KeyCode.A), "keycode down check 1");

        _keyboardControllerBase.MovieUpdate(true);

        RuntimeAssert.True(Input.GetKeyDown(KeyCode.A), "keycode down check 2");

        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetKeyDown(KeyCode.A), "keycode down check 3");

        _keyboardControllerBase.MovieUpdate(true);

        RuntimeAssert.False(Input.GetKeyDown(KeyCode.A), "keycode down check 4");

        _keyboardController.Release(KeyCode.A);
        _virtualEnvController.RunVirtualEnvironment = false;
    }

    [RuntimeTest]
    public void KeyDownTest2()
    {
        _virtualEnvController.RunVirtualEnvironment = true;

        _keyboardController.Hold(KeyCode.A);
        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.True(Input.GetKeyDown(KeyCode.A), "keycode down check 1");

        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetKeyDown(KeyCode.A), "keycode down check 2");

        _keyboardController.Release(KeyCode.A);
        _virtualEnvController.RunVirtualEnvironment = false;
    }

    [RuntimeTest]
    public void GetMousePress()
    {
        _virtualEnvController.RunVirtualEnvironment = true;

        RuntimeAssert.False(Input.GetMouseButton(0), "check 1");

        _mouseController.HoldButton(MouseButton.Left);
        _mouseControllerBase.MovieUpdate(false);

        RuntimeAssert.True(Input.GetMouseButtonDown(0), "check 2");
        RuntimeAssert.True(Input.GetMouseButton(0), "check 3");

        _mouseControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetMouseButtonDown(0), "check 4");
        RuntimeAssert.True(Input.GetMouseButton(0), "check 5");

        _mouseController.HoldButton(MouseButton.Middle);
        _mouseControllerBase.MovieUpdate(false);

        RuntimeAssert.True(Input.GetMouseButtonDown(2), "check 6");
        RuntimeAssert.True(Input.GetMouseButton(2), "check 7");

        _mouseController.ReleaseButton(MouseButton.Left);
        _mouseController.ReleaseButton(MouseButton.Middle);
        _mouseControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetMouseButtonDown(2), "check 8");
        RuntimeAssert.False(Input.GetMouseButton(0), "check 9");
        RuntimeAssert.True(Input.GetMouseButtonUp(0), "check 10");
        RuntimeAssert.True(Input.GetMouseButtonUp(2), "check 13");

        _mouseControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetMouseButtonUp(0), "check 14");
        RuntimeAssert.False(Input.GetMouseButtonUp(2), "check 15");

        _virtualEnvController.RunVirtualEnvironment = false;
    }
}