using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.Coroutine;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.RuntimeTest;
using UniTAS.Plugin.Models.Coroutine;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UniTAS.Plugin.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UniTAS.Plugin.RuntimeTests;

[Register]
public class NewInputSystemTests
{
    private readonly IMouseStateEnv _mouseStateEnv;
    private readonly IVirtualEnvController _virtualEnvController;

    public NewInputSystemTests(IMouseStateEnv mouseStateEnv, IVirtualEnvController virtualEnvController)
    {
        _mouseStateEnv = mouseStateEnv;
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
        _mouseStateEnv.Position = new(500, 600);

        yield return new WaitForUpdateUnconditional();

        // we wait for late update since input system update happens AFTER the update
        yield return new WaitForLastUpdateUnconditional();

        var mouse = Mouse.current;
        var pos = mouse.position.ReadValue();
        RuntimeAssert.AreEqual(500f, pos.x, "mouse x position check");
        RuntimeAssert.AreEqual(600f, pos.y, "mouse y position check");

        _virtualEnvController.RunVirtualEnvironment = false;
        _mouseStateEnv.Position = Vector2.zero;
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
        _mouseStateEnv.LeftClick = true;

        yield return new WaitForUpdateUnconditional();
        yield return new WaitForLastUpdateUnconditional();

        var mouse = Mouse.current;

        RuntimeAssert.True(mouse.leftButton.isPressed, "left button check");

        _mouseStateEnv.LeftClick = false;

        yield return new WaitForLastUpdateUnconditional();

        RuntimeAssert.False(mouse.leftButton.isPressed, "left button check");

        _mouseStateEnv.RightClick = true;

        yield return new WaitForLastUpdateUnconditional();

        RuntimeAssert.True(mouse.rightButton.isPressed, "right button check");

        _mouseStateEnv.RightClick = false;

        yield return new WaitForLastUpdateUnconditional();

        RuntimeAssert.False(mouse.rightButton.isPressed, "right button check");

        _mouseStateEnv.MiddleClick = true;

        yield return new WaitForLastUpdateUnconditional();

        RuntimeAssert.True(mouse.middleButton.isPressed, "middle button check");

        _mouseStateEnv.MiddleClick = false;

        yield return new WaitForLastUpdateUnconditional();

        RuntimeAssert.False(mouse.middleButton.isPressed, "middle button check");

        _virtualEnvController.RunVirtualEnvironment = false;
        inputSettings.updateMode = updateMode;
    }
}