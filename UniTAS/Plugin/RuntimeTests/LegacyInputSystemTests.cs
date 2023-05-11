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

namespace UniTAS.Plugin.RuntimeTests;

[Register]
public class LegacyInputSystemTests
{
    private readonly IKeyboardStateEnvController _keyboardController;
    private readonly IMouseStateEnvController _mouseController;
    private readonly IVirtualEnvController _virtualEnvController;
    private readonly IKeyFactory _keyFactory;
    private readonly ITimeEnv _timeEnv;

    public LegacyInputSystemTests(IKeyboardStateEnvController keyboardController,
        IVirtualEnvController virtualEnvController, IMouseStateEnvController mouseController, IKeyFactory keyFactory,
        ITimeEnv timeEnv)
    {
        _keyboardController = keyboardController;
        _virtualEnvController = virtualEnvController;
        _mouseController = mouseController;
        _keyFactory = keyFactory;
        _timeEnv = timeEnv;
    }

    [RuntimeTest]
    public IEnumerator<CoroutineWait> GetKeyTest()
    {
        _virtualEnvController.RunVirtualEnvironment = true;

        yield return new WaitForUpdateUnconditional();

        _keyboardController.Hold(_keyFactory.CreateKey(KeyCode.A));

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.True(Input.GetKeyDown(KeyCode.A), "keycode down check");
        RuntimeAssert.True(Input.GetKeyDown("a"), "string down check");
        RuntimeAssert.True(Input.GetKey(KeyCode.A), "keycode check");
        RuntimeAssert.True(Input.GetKey("a"), "string check");

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.False(Input.GetKeyDown(KeyCode.A), "keycode down check");
        RuntimeAssert.False(Input.GetKeyDown("a"), "string down check");
        RuntimeAssert.True(Input.GetKey(KeyCode.A), "keycode 2 check");
        RuntimeAssert.True(Input.GetKey("a"), "string 2 check");

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.True(Input.GetKey(KeyCode.A), "keycode 3 check");
        RuntimeAssert.True(Input.GetKey("a"), "string 3 check");

        _keyboardController.Release(_keyFactory.CreateKey(KeyCode.A));

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.False(Input.GetKey(KeyCode.A), "keycode 4 check");
        RuntimeAssert.False(Input.GetKey("a"), "string 4 check");
        RuntimeAssert.True(Input.GetKeyUp(KeyCode.A), "keycode up check");
        RuntimeAssert.True(Input.GetKeyUp("a"), "string up check");

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.False(Input.GetKeyUp(KeyCode.A), "keycode up check");
        RuntimeAssert.False(Input.GetKeyUp("a"), "string up check");

        _virtualEnvController.RunVirtualEnvironment = false;
    }

    [RuntimeTest]
    public IEnumerator<CoroutineWait> KeyDownTest()
    {
        _virtualEnvController.RunVirtualEnvironment = true;

        var captureFt = _timeEnv.FrameTime;
        var fixedDt = Time.fixedDeltaTime;

        _timeEnv.FrameTime = 0.01;
        Time.fixedDeltaTime = 0.02f;

        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        Plugin.Log.LogDebug("press A");
        _keyboardController.Hold(_keyFactory.CreateKey(KeyCode.A));

        yield return new WaitForFixedUpdateUnconditional();

        Plugin.Log.LogDebug("check A press");
        RuntimeAssert.True(Input.GetKeyDown(KeyCode.A), "keycode down check 1");

        yield return new WaitForUpdateUnconditional();

        Plugin.Log.LogDebug("check A press 2");
        RuntimeAssert.True(Input.GetKeyDown(KeyCode.A), "keycode down check 2");

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.False(Input.GetKeyDown(KeyCode.A), "keycode down check 3");

        yield return new WaitForFixedUpdateUnconditional();

        RuntimeAssert.False(Input.GetKeyDown(KeyCode.A), "keycode down check 4");

        _keyboardController.Release(_keyFactory.CreateKey(KeyCode.A));
        _timeEnv.FrameTime = captureFt;
        Time.fixedDeltaTime = fixedDt;
        _virtualEnvController.RunVirtualEnvironment = false;
    }

    [RuntimeTest]
    public IEnumerator<CoroutineWait> KeyDownTest2()
    {
        _virtualEnvController.RunVirtualEnvironment = true;

        var captureFt = _timeEnv.FrameTime;
        var fixedDt = Time.fixedDeltaTime;

        _timeEnv.FrameTime = 0.01;
        Time.fixedDeltaTime = 0.02f;

        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        Plugin.Log.LogDebug("press A");
        _keyboardController.Hold(_keyFactory.CreateKey(KeyCode.A));

        yield return new WaitForUpdateUnconditional();

        Plugin.Log.LogDebug("check A press");
        RuntimeAssert.True(Input.GetKeyDown(KeyCode.A), "keycode down check 1");

        yield return new WaitForFixedUpdateUnconditional();

        RuntimeAssert.False(Input.GetKeyDown(KeyCode.A), "keycode down check 2");

        _keyboardController.Release(_keyFactory.CreateKey(KeyCode.A));
        _timeEnv.FrameTime = captureFt;
        Time.fixedDeltaTime = fixedDt;
        _virtualEnvController.RunVirtualEnvironment = false;
    }

    [RuntimeTest]
    public IEnumerator<CoroutineWait> GetMousePress()
    {
        _virtualEnvController.RunVirtualEnvironment = true;

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.False(Input.GetMouseButton(0));

        _mouseController.HoldButton(MouseButton.Left);

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.True(Input.GetMouseButtonDown(0));
        RuntimeAssert.True(Input.GetMouseButton(0));

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.False(Input.GetMouseButtonDown(0));
        RuntimeAssert.True(Input.GetMouseButton(0));

        yield return new WaitForUpdateUnconditional();

        _mouseController.HoldButton(MouseButton.Middle);

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.True(Input.GetMouseButtonDown(2));
        RuntimeAssert.True(Input.GetMouseButton(2));

        yield return new WaitForUpdateUnconditional();

        _mouseController.ReleaseButton(MouseButton.Left);
        _mouseController.ReleaseButton(MouseButton.Middle);

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.False(Input.GetMouseButtonDown(0));
        RuntimeAssert.False(Input.GetMouseButton(0));
        RuntimeAssert.True(Input.GetMouseButtonUp(0));

        RuntimeAssert.False(Input.GetMouseButtonDown(2));
        RuntimeAssert.False(Input.GetMouseButton(2));
        RuntimeAssert.True(Input.GetMouseButtonUp(2));

        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.False(Input.GetMouseButtonUp(0));
        RuntimeAssert.False(Input.GetMouseButtonUp(2));

        _virtualEnvController.RunVirtualEnvironment = false;
    }
}