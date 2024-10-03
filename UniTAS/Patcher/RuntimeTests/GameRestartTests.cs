using System;
using System.Collections.Generic;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.RuntimeTest;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.RuntimeTest;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.RuntimeTests;

public class GameRestartTests
{
    private readonly IGameRestart _gameRestart;
    private readonly ITimeEnv _timeEnv;
    private readonly IUnityEnvTestingSave _unityEnvTestingSave;
    private readonly IUpdateInvokeOffset _updateInvokeOffset;

    private const float TIME_PRECISION = 0.000001f;

    public GameRestartTests(IGameRestart gameRestart, ITimeEnv timeEnv, IUnityEnvTestingSave unityEnvTestingSave,
        IUpdateInvokeOffset updateInvokeOffset)
    {
        _gameRestart = gameRestart;
        _timeEnv = timeEnv;
        _unityEnvTestingSave = unityEnvTestingSave;
        _updateInvokeOffset = updateInvokeOffset;
    }

    [RuntimeTest]
    public IEnumerable<CoroutineWait> SoftRestart100FPS()
    {
        yield return new WaitForCoroutine(InitTest(0.01f));

        // FixedUpdate is the first thing
        yield return new WaitForFixedUpdateActual();

        // don't test offset here, it doesn't matter as long as it syncs up properly in a bit
        RuntimeAssert.FloatEquals(0f, Time.time, TIME_PRECISION, "Assert 1");

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(0.01f, (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime,
            TIME_PRECISION,
            "Didn't match the offset at the first update");

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.FloatEquals(0.01f, (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime,
            TIME_PRECISION,
            "Didn't match the offset at the first fixed update");

        RuntimeAssert.FloatEquals(0.02f, Time.time, TIME_PRECISION);

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(0f, (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime,
            TIME_PRECISION,
            "Didn't match the offset at the second update");

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(0.01f, (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime,
            TIME_PRECISION,
            "Didn't match the offset at the third update");

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.FloatEquals(0.01f, (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime,
            TIME_PRECISION,
            "Didn't match the offset at the second fixed update");

        yield return new WaitForCoroutine(CleanupTest());
    }

    [RuntimeTest]
    public IEnumerable<CoroutineWait> SoftRestart60FPS()
    {
        yield return new WaitForCoroutine(InitTest(1f / 60f));

        // FixedUpdate is the first thing
        yield return new WaitForFixedUpdateActual();

        // don't test offset here, it doesn't matter as long as it syncs up properly in a bit
        RuntimeAssert.FloatEquals(0f, Time.time, TIME_PRECISION, "Assert 1");

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f, (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime,
            TIME_PRECISION, "Assert 2");

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f, (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime,
            TIME_PRECISION, "Assert 3");

        RuntimeAssert.FloatEquals(0.02f, Time.time, TIME_PRECISION, "Assert 4");

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f * 2f % Time.fixedDeltaTime,
            (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime, TIME_PRECISION, "Assert 5");

        yield return new WaitForFixedUpdateActual();

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f * 3f % Time.fixedDeltaTime,
            (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime, TIME_PRECISION, "Assert 6");

        yield return new WaitForFixedUpdateActual();

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f * 4f % Time.fixedDeltaTime,
            (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime, TIME_PRECISION, "Assert 7");

        yield return new WaitForFixedUpdateActual();

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f * 5f % Time.fixedDeltaTime,
            (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime, TIME_PRECISION, "Assert 8");

        yield return new WaitForFixedUpdateActual();

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f * 6f % Time.fixedDeltaTime,
            (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime, TIME_PRECISION, "Assert 9");

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f * 7f % Time.fixedDeltaTime,
            (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime, TIME_PRECISION, "Assert 10");

        yield return new WaitForCoroutine(CleanupTest());
    }

    // [RuntimeTest]
    // TODO: this test can't be tested in a "clean" way because games aren't usually at this weir deltaTime
    // move to a more proper testing environment
    public IEnumerable<CoroutineWait> SoftRestart100FPSWeirdFixedDeltaTime()
    {
        /* The update order with time looks like this (tested with a real game)
         * ==========
         * f 0
         * u 0.01
         * u 0.02
         * f 0.025
         * u 0.03
         * u 0.04
         * u 0.05
         * f 0.05
         * u 0.06
         * u 0.07
         * f 0.075
         * u 0.08
         * ==========
         */

        const float fixedDeltaTime = 0.025f;

        yield return new WaitForCoroutine(InitTest(0.01f, fixedDeltaTime));

        // FixedUpdate is the first thing
        yield return new WaitForFixedUpdateActual();

        // don't test offset here, it doesn't matter as long as it syncs up properly in a bit
        RuntimeAssert.FloatEquals(0f, Time.time, TIME_PRECISION, "Assert 1");

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 100f, (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime,
            TIME_PRECISION, "Assert 2");

        RuntimeAssert.FloatEquals(0.01f, Time.time, TIME_PRECISION, "Assert 3");

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 100f * 2f, (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime,
            TIME_PRECISION, "Assert 4");

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.FloatEquals(1f / 100f * 2f, (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime,
            TIME_PRECISION, "Assert 5");

        RuntimeAssert.FloatEquals(fixedDeltaTime, Time.time, TIME_PRECISION, "Assert 6");

        // triple update in a row
        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 100f * 3f % Time.fixedDeltaTime,
            (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime, TIME_PRECISION, "Assert 7");

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 100f * 4f % Time.fixedDeltaTime,
            (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime, TIME_PRECISION, "Assert 8");

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 100f * 5f % Time.fixedDeltaTime,
            (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime, TIME_PRECISION, "Assert 10");

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.FloatEquals(fixedDeltaTime * 2f, Time.time, TIME_PRECISION, "Assert 9");

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 100f * 6f % Time.fixedDeltaTime,
            (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime, TIME_PRECISION, "Assert 11");

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 100f * 7f % Time.fixedDeltaTime,
            (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime, TIME_PRECISION, "Assert 12");

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.FloatEquals(fixedDeltaTime * 3f, Time.time, TIME_PRECISION, "Assert 13");

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 100f * 8f % Time.fixedDeltaTime,
            (float)_updateInvokeOffset.Offset % Time.fixedDeltaTime, TIME_PRECISION, "Assert 14");

        yield return new WaitForCoroutine(CleanupTest());
    }

    private IEnumerable<CoroutineWait> InitTest(float ft, float fixedDeltaTime = 0.02f)
    {
        // for testing the occasional double fixed update invokes
        var waitManual = new WaitManual();

        _gameRestart.OnGameRestartResume += WaitManualCallback;

        _unityEnvTestingSave.Save();

        _timeEnv.FrameTime = ft;
        Time.fixedDeltaTime = fixedDeltaTime;
        Time.maximumDeltaTime = 1f / 3f;
        Time.timeScale = 1f;

        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        _gameRestart.SoftRestart(DateTime.Now);

        // wait for soft restart
        yield return waitManual;
        _gameRestart.OnGameRestartResume -= WaitManualCallback;
        yield break;

        void WaitManualCallback(DateTime dateTime, bool preMonoBehaviourResume)
        {
            if (!preMonoBehaviourResume)
            {
                waitManual.RunNext();
            }
        }
    }

    private IEnumerable<CoroutineWait> CleanupTest()
    {
        _unityEnvTestingSave.Restore();

        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
    }
}
