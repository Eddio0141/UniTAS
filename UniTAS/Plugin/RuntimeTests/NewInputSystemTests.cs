using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.Coroutine;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.RuntimeTest;
using UniTAS.Plugin.Models.Coroutine;
using UniTAS.Plugin.Models.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UniTAS.Plugin.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UniTAS.Plugin.RuntimeTests;

[Register]
public class NewInputSystemTests
{
    private readonly IMouseStateEnvController _mouseController;
    private readonly IVirtualEnvController _virtualEnvController;

    public NewInputSystemTests(IMouseStateEnvController mouseController, IVirtualEnvController virtualEnvController)
    {
        _mouseController = mouseController;
        _virtualEnvController = virtualEnvController;
    }

    [RuntimeTest]
    public IEnumerator<CoroutineWait> MousePosition()
    {
        // TODO skip if no input system
        var inputSettings = InputSystem.settings;
        var updateMode = inputSettings.updateMode;
        inputSettings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
        _virtualEnvController.RunVirtualEnvironment = true;

        yield return new WaitForLastUpdateUnconditional();

        _mouseController.SetPosition(new(500, 600));

        yield return new WaitForUpdateUnconditional();

        var mouse = Mouse.current;
        var pos = mouse.position.ReadValue();
        RuntimeAssert.AreEqual(500f, pos.x, "mouse x position check");
        RuntimeAssert.AreEqual(600f, pos.y, "mouse y position check");

        _virtualEnvController.RunVirtualEnvironment = false;
        _mouseController.SetPosition(Vector2.zero);
        inputSettings.updateMode = updateMode;
    }

    [RuntimeTest]
    public IEnumerator<CoroutineWait> MouseButtons()
    {
        // TODO skip if no input system
        var inputSettings = InputSystem.settings;
        var updateMode = inputSettings.updateMode;
        inputSettings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
        _virtualEnvController.RunVirtualEnvironment = true;

        yield return new WaitForLastUpdateUnconditional();

        _mouseController.HoldButton(MouseButton.Left);

        yield return new WaitForUpdateUnconditional();

        var mouse = Mouse.current;

        RuntimeAssert.True(mouse.leftButton.isPressed, "left button check");

        _mouseController.ReleaseButton(MouseButton.Left);

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.False(mouse.leftButton.isPressed, "left button check");

        _mouseController.HoldButton(MouseButton.Right);

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.True(mouse.rightButton.isPressed, "right button check");

        _mouseController.ReleaseButton(MouseButton.Right);

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.False(mouse.rightButton.isPressed, "right button check");

        _mouseController.HoldButton(MouseButton.Middle);

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.True(mouse.middleButton.isPressed, "middle button check");

        _mouseController.ReleaseButton(MouseButton.Middle);

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.False(mouse.middleButton.isPressed, "middle button check");

        _virtualEnvController.RunVirtualEnvironment = false;
        inputSettings.updateMode = updateMode;
    }
}