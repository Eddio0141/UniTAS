using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.Coroutine;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.RuntimeTest;
using UniTAS.Plugin.Models.Coroutine;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UniTAS.Plugin.Utils;
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
        yield return new WaitForUpdateUnconditional();

        // TODO skip if no input system
        _virtualEnvController.RunVirtualEnvironment = true;

        yield return new WaitForUpdateUnconditional();

        _mouseStateEnv.XPos = 500;
        _mouseStateEnv.YPos = 600;

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.AreEqual(500, Mouse.current.position.x.ReadValue(), "mouse x position check");
        RuntimeAssert.AreEqual(600, Mouse.current.position.y.ReadValue(), "mouse y position check");

        _virtualEnvController.RunVirtualEnvironment = false;
        _mouseStateEnv.XPos = 0;
        _mouseStateEnv.YPos = 0;
    }

    [RuntimeTest]
    public IEnumerator<CoroutineWait> MouseButtons()
    {
        yield return new WaitForUpdateUnconditional();

        // TODO skip if no input system
        _virtualEnvController.RunVirtualEnvironment = true;

        yield return new WaitForUpdateUnconditional();

        _mouseStateEnv.LeftClick = true;
        var mouse = Mouse.current;

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.True(mouse.leftButton.isPressed, "left button check");

        _mouseStateEnv.LeftClick = false;

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.False(mouse.leftButton.isPressed, "left button check");

        _mouseStateEnv.RightClick = true;

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.True(mouse.rightButton.isPressed, "right button check");

        _mouseStateEnv.RightClick = false;

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.False(mouse.rightButton.isPressed, "right button check");

        _mouseStateEnv.MiddleClick = true;

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.True(mouse.middleButton.isPressed, "middle button check");

        _mouseStateEnv.MiddleClick = false;

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.False(mouse.middleButton.isPressed, "middle button check");

        _virtualEnvController.RunVirtualEnvironment = false;
    }
}