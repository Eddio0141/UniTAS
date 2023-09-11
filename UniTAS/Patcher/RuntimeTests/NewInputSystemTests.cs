using System.Collections.Generic;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.RuntimeTest;
using UniTAS.Patcher.Models.Utils;
using UniTAS.Patcher.Models.VirtualEnvironment;
using UniTAS.Patcher.Services.InputSystemOverride;
using UniTAS.Patcher.Services.NewInputSystem;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Services.VirtualEnvironment.Input;
using UniTAS.Patcher.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UniTAS.Patcher.RuntimeTests;

[Register]
public class NewInputSystemTests
{
    private readonly IMouseStateEnvController _mouseController;
    private readonly IVirtualEnvController _virtualEnvController;
    private readonly INewInputSystemExists _newInputSystemExists;
    private readonly IInputSystemOverride _inputSystemOverride;

    public NewInputSystemTests(IMouseStateEnvController mouseController, IVirtualEnvController virtualEnvController,
        INewInputSystemExists newInputSystemExists, IInputSystemOverride inputSystemOverride)
    {
        _mouseController = mouseController;
        _virtualEnvController = virtualEnvController;
        _newInputSystemExists = newInputSystemExists;
        _inputSystemOverride = inputSystemOverride;
    }

    [RuntimeTest]
    public Tuple<bool, IEnumerable<CoroutineWait>> MousePosition()
    {
        return new(_newInputSystemExists.HasInputSystem, MousePositionInternal());
    }

    private IEnumerable<CoroutineWait> MousePositionInternal()
    {
        var inputSettings = InputSystem.settings;
        var updateMode = inputSettings.updateMode;
        inputSettings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
        _virtualEnvController.RunVirtualEnvironment = true;
        _inputSystemOverride.Override = true;

        yield return new WaitForLastUpdateUnconditional();

        _mouseController.SetPosition(new(500, 600));

        yield return new WaitForUpdateUnconditional();

        var mouse = Mouse.current;
        var pos = mouse.position.ReadValue();
        RuntimeAssert.AreEqual(500f, pos.x, "mouse x position check");
        RuntimeAssert.AreEqual(600f, pos.y, "mouse y position check");

        _virtualEnvController.RunVirtualEnvironment = false;
        _inputSystemOverride.Override = false;
        _mouseController.SetPosition(Vector2.zero);
        inputSettings.updateMode = updateMode;
    }

    [RuntimeTest]
    public Tuple<bool, IEnumerable<CoroutineWait>> MouseButtons()
    {
        return new(_newInputSystemExists.HasInputSystem, MouseButtonsInternal());
    }

    private IEnumerable<CoroutineWait> MouseButtonsInternal()
    {
        var inputSettings = InputSystem.settings;
        var updateMode = inputSettings.updateMode;
        inputSettings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
        _virtualEnvController.RunVirtualEnvironment = true;
        _inputSystemOverride.Override = true;

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
        _inputSystemOverride.Override = false;
        inputSettings.updateMode = updateMode;
    }
}