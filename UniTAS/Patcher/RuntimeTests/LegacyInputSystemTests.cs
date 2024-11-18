using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.RuntimeTest;
using UniTAS.Patcher.Interfaces.VirtualEnvironment;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input.LegacyInputSystem;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.RuntimeTests;

[Register]
public class LegacyInputSystemTests(
    IKeyboardStateEnvLegacySystem keyboardController,
    IVirtualEnvController virtualEnvController,
    IMouseStateEnvLegacySystem mouseController,
    IInputSystemState inputSystemState)
{
    private readonly LegacyInputSystemDevice _keyboardControllerBase = (LegacyInputSystemDevice)keyboardController;

    private readonly LegacyInputSystemDevice _mouseControllerBase = (LegacyInputSystemDevice)mouseController;

    [RuntimeTest]
    public bool GetKeyTest()
    {
        if (!inputSystemState.HasOldInputSystem) return false;

        virtualEnvController.RunVirtualEnvironment = true;

        keyboardController.Hold(KeyCode.A);
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

        keyboardController.Release(KeyCode.A);
        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetKey(KeyCode.A), "keycode 4 check");
        RuntimeAssert.False(Input.GetKey("a"), "string 4 check");
        RuntimeAssert.True(Input.GetKeyUp(KeyCode.A), "keycode up check");
        RuntimeAssert.True(Input.GetKeyUp("a"), "string up check");

        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetKeyUp(KeyCode.A), "keycode up check");
        RuntimeAssert.False(Input.GetKeyUp("a"), "string up check");

        virtualEnvController.RunVirtualEnvironment = false;
        return true;
    }

    [RuntimeTest]
    public bool KeyDownTest()
    {
        if (!inputSystemState.HasOldInputSystem) return false;
        virtualEnvController.RunVirtualEnvironment = true;

        keyboardController.Hold(KeyCode.A);
        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.True(Input.GetKeyDown(KeyCode.A), "keycode down check 1");

        _keyboardControllerBase.MovieUpdate(true);

        RuntimeAssert.True(Input.GetKeyDown(KeyCode.A), "keycode down check 2");

        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetKeyDown(KeyCode.A), "keycode down check 3");

        _keyboardControllerBase.MovieUpdate(true);

        RuntimeAssert.False(Input.GetKeyDown(KeyCode.A), "keycode down check 4");

        keyboardController.Release(KeyCode.A);
        virtualEnvController.RunVirtualEnvironment = false;
        return true;
    }

    [RuntimeTest]
    public bool KeyDownTest2()
    {
        if (!inputSystemState.HasOldInputSystem) return false;
        virtualEnvController.RunVirtualEnvironment = true;

        keyboardController.Hold(KeyCode.A);
        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.True(Input.GetKeyDown(KeyCode.A), "keycode down check 1");

        _keyboardControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetKeyDown(KeyCode.A), "keycode down check 2");

        keyboardController.Release(KeyCode.A);
        virtualEnvController.RunVirtualEnvironment = false;
        return true;
    }

    [RuntimeTest]
    public bool GetMousePress()
    {
        if (!inputSystemState.HasOldInputSystem) return false;
        virtualEnvController.RunVirtualEnvironment = true;

        RuntimeAssert.False(Input.GetMouseButton(0), "check 1");

        mouseController.HoldButton(MouseButton.Left);
        _mouseControllerBase.MovieUpdate(false);

        RuntimeAssert.True(Input.GetMouseButtonDown(0), "check 2");
        RuntimeAssert.True(Input.GetMouseButton(0), "check 3");

        _mouseControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetMouseButtonDown(0), "check 4");
        RuntimeAssert.True(Input.GetMouseButton(0), "check 5");

        mouseController.HoldButton(MouseButton.Middle);
        _mouseControllerBase.MovieUpdate(false);

        RuntimeAssert.True(Input.GetMouseButtonDown(2), "check 6");
        RuntimeAssert.True(Input.GetMouseButton(2), "check 7");

        mouseController.ReleaseButton(MouseButton.Left);
        mouseController.ReleaseButton(MouseButton.Middle);
        _mouseControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetMouseButtonDown(2), "check 8");
        RuntimeAssert.False(Input.GetMouseButton(0), "check 9");
        RuntimeAssert.True(Input.GetMouseButtonUp(0), "check 10");
        RuntimeAssert.True(Input.GetMouseButtonUp(2), "check 13");

        _mouseControllerBase.MovieUpdate(false);

        RuntimeAssert.False(Input.GetMouseButtonUp(0), "check 14");
        RuntimeAssert.False(Input.GetMouseButtonUp(2), "check 15");

        virtualEnvController.RunVirtualEnvironment = false;
        return true;
    }

    // TODO: move this to movie test
    // [RuntimeTest]
    // public bool MouseLeftClickToAxis()
    // {
    //     if (!inputSystemState.HasOldInputSystem) return false;
    //     virtualEnvController.RunVirtualEnvironment = true;
    //
    //     // check if the axis exists
    //     var allAxis = axisStateEnvLegacySystem._values;
    //     var clickAxisIndex = -1;
    //     for (var i = 0; i < allAxis.Count; i++)
    //     {
    //         var axis = allAxis[i].Item2.Axis;
    //         if (axis.PositiveButton == "mouse 0" || axis.AltPositiveButton == "mouse 0" ||
    //             axis.NegativeButton == "mouse 0" || axis.AltNegativeButton == "mouse 0")
    //         {
    //             clickAxisIndex = i;
    //             break;
    //         }
    //     }
    //
    //     if (clickAxisIndex < 0) return false;
    //
    //     var (clickAxis, _) = allAxis[clickAxisIndex];
    //
    //     mouse.Left();
    //     _mouseControllerBase.MovieUpdate(false);
    //
    //     RuntimeAssert.FloatEquals(1f, Input.GetAxisRaw(clickAxis), 0.01f, "axis == 1");
    //
    //     mouse.Left(false);
    //     _mouseControllerBase.MovieUpdate(false);
    //
    //     RuntimeAssert.FloatEquals(0f, Input.GetAxisRaw(clickAxis), 0.01f, "axis == 0");
    //
    //     virtualEnvController.RunVirtualEnvironment = false;
    //
    //     return true;
    // }

    // TODO: movie test
    // [RuntimeTest]
    // public bool MouseRightClickToAxis()
    // {
    //     if (!inputSystemState.HasOldInputSystem) return false;
    //     virtualEnvController.RunVirtualEnvironment = true;
    //
    //     // check if the axis exists
    //     var allAxis = axisStateEnvLegacySystem._values;
    //     var clickAxisIndex = -1;
    //     for (var i = 0; i < allAxis.Count; i++)
    //     {
    //         var axis = allAxis[i].Item2.Axis;
    //         if (axis.PositiveButton == "mouse 1" || axis.AltPositiveButton == "mouse 1" ||
    //             axis.NegativeButton == "mouse 1" || axis.AltNegativeButton == "mouse 1")
    //         {
    //             clickAxisIndex = i;
    //             break;
    //         }
    //     }
    //
    //     if (clickAxisIndex < 0) return false;
    //
    //     var (clickAxis, _) = allAxis[clickAxisIndex];
    //
    //     mouse.Right();
    //     _mouseControllerBase.MovieUpdate(false);
    //
    //     RuntimeAssert.FloatEquals(1f, Input.GetAxisRaw(clickAxis), 0.01f, "axis == 1");
    //
    //     mouse.Right(false);
    //     _mouseControllerBase.MovieUpdate(false);
    //
    //     RuntimeAssert.FloatEquals(0f, Input.GetAxisRaw(clickAxis), 0.01f, "axis == 0");
    //
    //     virtualEnvController.RunVirtualEnvironment = false;
    //
    //     return true;
    // }
}