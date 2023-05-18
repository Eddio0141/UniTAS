using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.Coroutine;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.RuntimeTest;
using UniTAS.Plugin.Models.Coroutine;
using UniTAS.Plugin.Models.VirtualEnvironment;
using UniTAS.Plugin.Services.InputSystemOverride;
using UniTAS.Plugin.Services.NewInputSystem;
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
    public Tuple<bool, IEnumerator<CoroutineWait>> MousePosition()
    {
        return new(_newInputSystemExists.HasInputSystem, MousePositionInternal());
    }

    private IEnumerator<CoroutineWait> MousePositionInternal()
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
    public Tuple<bool, IEnumerator<CoroutineWait>> MouseButtons()
    {
        return new(_newInputSystemExists.HasInputSystem, MouseButtonsInternal());
    }

    private IEnumerator<CoroutineWait> MouseButtonsInternal()
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