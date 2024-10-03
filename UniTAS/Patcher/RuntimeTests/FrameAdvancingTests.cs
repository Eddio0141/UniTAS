using System;
using System.Collections.Generic;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Implementations.FrameAdvancing;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.RuntimeTest;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.FrameAdvancing;
using UniTAS.Patcher.Services.RuntimeTest;
using UniTAS.Patcher.Services.UnityEvents;
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
    private readonly IGameRestart _gameRestart;

    public FrameAdvancingTests(ITimeEnv timeEnv, IFrameAdvancing frameAdvancing, IUpdateEvents updateEvents,
        IUnityEnvTestingSave unityEnvTestingSave, IGameRestart gameRestart)
    {
        _timeEnv = timeEnv;
        _frameAdvancing = frameAdvancing;
        _updateEvents = updateEvents;
        _unityEnvTestingSave = unityEnvTestingSave;
        _gameRestart = gameRestart;
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

        var waitManual = new WaitManual();
        _gameRestart.OnGameRestartResume += WaitManualCallback;
        _gameRestart.SoftRestart(DateTime.Now);

        yield return waitManual;
        _gameRestart.OnGameRestartResume -= WaitManualCallback;

        void WaitManualCallback(DateTime dateTime, bool preMonoBehaviourResume)
        {
            if (!preMonoBehaviourResume)
            {
                waitManual.RunNext();
            }
        }
    }

    private IEnumerable<CoroutineWait> CleanUp()
    {
        _frameAdvancing.TogglePause();
        _unityEnvTestingSave.Restore();
        yield return FrameAdvanceWaits;
    }

    [RuntimeTest]
    public IEnumerable<CoroutineWait> FrameAdvanceFirstPause()
    {
        yield return new WaitForCoroutine(Init(0.01, 0.02f));

        var actualUpdateCounter = 0;
        var actualFixedUpdateCounter = 0;

        // is it paused?
        // this should pause it instantly
        _frameAdvancing.FrameAdvance(2, FrameAdvanceMode.Update);

        _updateEvents.AddPriorityCallback(CallbackUpdate.UpdateActual, UpdateCounter,
            CallbackPriority.FrameAdvancingTest);
        _updateEvents.OnFixedUpdateActual += FixedUpdateCounter;

        yield return FrameAdvanceWaits;

        // because we don't pause on FixedUpdate, it advances by 1 before the first Update
        RuntimeAssert.AreEqual(1, actualFixedUpdateCounter, "Assert 1");
        RuntimeAssert.AreEqual(0, actualUpdateCounter, "Assert 2");

        yield return new WaitForCoroutine(CleanUp());

        _updateEvents.OnUpdateActual -= UpdateCounter;
        _updateEvents.OnFixedUpdateActual -= FixedUpdateCounter;

        yield break;

        void UpdateCounter()
        {
            actualUpdateCounter++;
        }

        void FixedUpdateCounter()
        {
            actualFixedUpdateCounter++;
        }
    }

    [RuntimeTest]
    public IEnumerable<CoroutineWait> FrameAdvanceUpdate()
    {
        yield return new WaitForCoroutine(Init(0.01, 0.02f));

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update);

        // the initial pause
        yield return FrameAdvanceWaits;

        // actual frame advance
        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update);

        yield return new WaitForUpdateActual();

        // time should be advanced
        RuntimeAssert.AreEqual(0.01f, Time.time, "assert 1");

        yield return FrameAdvanceWaits;

        // =====

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update);

        yield return new WaitForUpdateActual();

        RuntimeAssert.AreEqual(0.02f, Time.time, "assert 2");

        yield return FrameAdvanceWaits;

        // =====

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update);

        yield return new WaitForUpdateActual();

        RuntimeAssert.AreEqual(0.03f, Time.time, "assert 3");

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
        yield return FrameAdvanceWaits;

        // actual frame advance
        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.FixedUpdate);

        yield return new WaitForFixedUpdateActual();

        // time should be advanced
        RuntimeAssert.AreEqual(0f, Time.time, "assert 1");

        yield return FrameAdvanceWaits;

        // =====

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.FixedUpdate);

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.AreEqual(0.02f, Time.time, "assert 2");

        yield return FrameAdvanceWaits;

        // =====

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.FixedUpdate);

        yield return new WaitForFixedUpdateActual();

        // time should be advanced
        RuntimeAssert.AreEqual(0.04f, Time.time, "assert 3");

        yield return FrameAdvanceWaits;

        // =====

        yield return new WaitForCoroutine(CleanUp());
    }

    [RuntimeTest]
    public IEnumerable<CoroutineWait> FrameAdvanceUpdateAndFixedUpdate()
    {
        // f <- first pause
        // u 0.01
        // f
        // u 0
        // u 0.01
        // f

        yield return new WaitForCoroutine(Init(0.01, 0.02f));

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update | FrameAdvanceMode.FixedUpdate);

        // the initial pause, we are at FixedUpdate
        yield return FrameAdvanceWaits;

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update | FrameAdvanceMode.FixedUpdate);

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.AreEqual(0f, Time.time, "assert 1");

        yield return FrameAdvanceWaits;

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update | FrameAdvanceMode.FixedUpdate);

        yield return new WaitForUpdateActual();

        RuntimeAssert.AreEqual(0.01f, Time.time, "assert 2");

        yield return FrameAdvanceWaits;

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update | FrameAdvanceMode.FixedUpdate);

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.AreEqual(0.02f, Time.time, "assert 3");

        yield return FrameAdvanceWaits;

        // =====

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update | FrameAdvanceMode.FixedUpdate);

        yield return new WaitForUpdateActual();

        RuntimeAssert.AreEqual(0.02f, Time.time, "assert 4");

        yield return FrameAdvanceWaits;

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update | FrameAdvanceMode.FixedUpdate);

        yield return new WaitForUpdateActual();

        RuntimeAssert.AreEqual(0.03f, Time.time, "assert 5");

        yield return FrameAdvanceWaits;

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update | FrameAdvanceMode.FixedUpdate);

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.AreEqual(0.04f, Time.time, "assert 6");

        yield return FrameAdvanceWaits;

        // =====

        yield return new WaitForCoroutine(CleanUp());
    }

    private static WaitForCoroutine FrameAdvanceWaits => new(FrameAdvanceWaitsCoroutine());

    private static IEnumerable<CoroutineWait> FrameAdvanceWaitsCoroutine()
    {
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
    }

    // [RuntimeTest]
    // public void MaximumDeltaTimeTest()
    // {
    // }
}