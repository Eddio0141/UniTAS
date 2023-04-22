using System.Collections.Generic;
using UniTAS.Plugin.Interfaces.Coroutine;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.RuntimeTest;
using UniTAS.Plugin.Models.Coroutine;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UniTAS.Plugin.Services.VirtualEnvironment.Input;
using UniTAS.Plugin.Utils;
using UnityEngine;

namespace UniTAS.Plugin.RuntimeTests;

[Register]
public class LegacyInputSystemTests
{
    private readonly IKeyboardStateEnv _keyboardStateEnv;
    private readonly IVirtualEnvController _virtualEnvController;

    public LegacyInputSystemTests(IKeyboardStateEnv keyboardStateEnv, IVirtualEnvController virtualEnvController)
    {
        _keyboardStateEnv = keyboardStateEnv;
        _virtualEnvController = virtualEnvController;
    }

    [RuntimeTest]
    public IEnumerator<CoroutineWait> GetKeyTest()
    {
        yield return new WaitForUpdateUnconditional();

        _virtualEnvController.RunVirtualEnvironment = true;

        yield return new WaitForUpdateUnconditional();
        _keyboardStateEnv.Hold(new(KeyCode.A));
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

        _keyboardStateEnv.Release(new(KeyCode.A));

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
}