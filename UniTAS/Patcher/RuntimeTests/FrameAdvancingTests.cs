using System.Collections.Generic;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Implementations.FrameAdvancing;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.RuntimeTest;
using UniTAS.Patcher.Services.EventSubscribers;
using UniTAS.Patcher.Services.FrameAdvancing;
using UniTAS.Patcher.Services.RuntimeTest;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.RuntimeTests;

[Register]
public class FrameAdvancingTests
{
    private readonly ITimeEnv _timeEnv;
    private readonly IFrameAdvancing _frameAdvancing;
    private readonly IUpdateEvents _updateEvents;
    private readonly IUnityEnvTestingSave _unityEnvTestingSave;

    public FrameAdvancingTests(ITimeEnv timeEnv, IFrameAdvancing frameAdvancing, IUpdateEvents updateEvents,
        IUnityEnvTestingSave unityEnvTestingSave)
    {
        _timeEnv = timeEnv;
        _frameAdvancing = frameAdvancing;
        _updateEvents = updateEvents;
        _unityEnvTestingSave = unityEnvTestingSave;
    }

    private IEnumerable<CoroutineWait> Init(double ft, float fixedDt)
    {
        _unityEnvTestingSave.Save();

        _timeEnv.FrameTime = ft;
        Time.fixedDeltaTime = fixedDt;
        Time.maximumDeltaTime = 1f / 3f;
        Time.timeScale = 1f;

        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForOnSync();
    }

    private IEnumerable<CoroutineWait> CleanUp()
    {
        _frameAdvancing.TogglePause();
        _unityEnvTestingSave.Restore();
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
    }

    [RuntimeTest]
    public IEnumerable<CoroutineWait> FrameAdvanceFirstPause()
    {
        yield return new WaitForCoroutine(Init(0.01, 0.02f));

        var actualUpdateCounter = 0;

        // is it paused?
        // this should pause it instantly
        _frameAdvancing.FrameAdvance(2, FrameAdvanceMode.Update);
        _updateEvents.OnUpdateActual += () => { actualUpdateCounter++; };

        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.AreEqual(1, actualUpdateCounter, "Update counter should be 0 since frame advance is activated");

        yield return new WaitForCoroutine(CleanUp());
    }

    [RuntimeTest]
    public IEnumerable<CoroutineWait> FrameAdvanceUpdate()
    {
        yield return new WaitForCoroutine(Init(0.01, 0.02f));

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update);

        // the initial pause
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        // actual frame advance
        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update);

        yield return new WaitForUpdateActual();

        // time should be advanced
        RuntimeAssert.AreEqual(_timeEnv.FrameTime, UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            "assert 1");

        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        // =====

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update);

        yield return new WaitForUpdateActual();

        RuntimeAssert.AreEqual(_timeEnv.FrameTime * 2 % Time.fixedDeltaTime,
            UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            "assert ");

        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        // =====

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update);

        yield return new WaitForUpdateActual();

        RuntimeAssert.AreEqual(_timeEnv.FrameTime * 2 % Time.fixedDeltaTime,
            UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            "assert 3");

        yield return new WaitForCoroutine(CleanUp());
    }

    [RuntimeTest]
    public IEnumerable<CoroutineWait> FrameAdvanceFixedUpdate()
    {
        // f
        // u <- sync, 0
        // u 0.01
        // f 0 <- pause
        // u 0
        // u 0.01
        // f <- frame advance

        yield return new WaitForCoroutine(Init(0.01, 0.02f));

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.FixedUpdate);

        // the initial pause
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForFixedUpdateUnconditional();

        // actual frame advance
        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.FixedUpdate);

        yield return new WaitForFixedUpdateActual();

        // time should be advanced
        RuntimeAssert.AreEqual(_timeEnv.FrameTime, UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            "assert 1");

        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        // =====

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.FixedUpdate);

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.AreEqual(_timeEnv.FrameTime, UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            "assert 2");

        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        // =====

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.FixedUpdate);

        yield return new WaitForFixedUpdateActual();

        // time should be advanced
        RuntimeAssert.AreEqual(_timeEnv.FrameTime, UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            "assert 3");

        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        // =====

        yield return new WaitForCoroutine(CleanUp());
    }

    [RuntimeTest]
    public IEnumerable<CoroutineWait> FrameAdvanceUpdateAndFixedUpdate()
    {
        // f
        // u <- sync, 0
        // u 0.01 <- first pause
        // f
        // u 0
        // u 0.01
        // f

        yield return new WaitForCoroutine(Init(0.01, 0.02f));

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update | FrameAdvanceMode.FixedUpdate);

        // the initial pause
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update | FrameAdvanceMode.FixedUpdate);

        yield return new WaitForUpdateActual();

        RuntimeAssert.AreEqual(_timeEnv.FrameTime, UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            "assert 1");

        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update | FrameAdvanceMode.FixedUpdate);

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.AreEqual(_timeEnv.FrameTime, UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            "assert 2");

        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        // =====

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update | FrameAdvanceMode.FixedUpdate);

        yield return new WaitForUpdateActual();

        RuntimeAssert.AreEqual(_timeEnv.FrameTime * 2 % Time.fixedDeltaTime,
            UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            "assert 3");

        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update | FrameAdvanceMode.FixedUpdate);

        yield return new WaitForUpdateActual();

        RuntimeAssert.AreEqual(_timeEnv.FrameTime * 3 % Time.fixedDeltaTime,
            UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            "assert 4");

        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update | FrameAdvanceMode.FixedUpdate);

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.AreEqual(_timeEnv.FrameTime * 3 % Time.fixedDeltaTime,
            UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            "assert 5");

        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        // =====

        yield return new WaitForCoroutine(CleanUp());
    }

    // [RuntimeTest]
    // public void MaximumDeltaTimeTest()
    // {
    // }
}