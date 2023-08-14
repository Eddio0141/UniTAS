using System;
using System.Collections.Generic;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Interfaces.Coroutine;
using UniTAS.Patcher.Interfaces.RuntimeTest;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.RuntimeTests;

public class GameRestartTests
{
    private readonly IGameRestart _gameRestart;
    private readonly ITimeEnv _timeEnv;

    public GameRestartTests(IGameRestart gameRestart, ITimeEnv timeEnv)
    {
        _gameRestart = gameRestart;
        _timeEnv = timeEnv;
    }

    [RuntimeTest]
    public IEnumerable<CoroutineWait> SoftRestart100FPS()
    {
        yield return new WaitForCoroutine(InitTest(0.01f));

        // FixedUpdate is the first thing
        yield return new WaitForFixedUpdateActual();

        // don't test offset here, it doesn't matter as long as it syncs up properly in a bit

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(0.01f, (float)UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            0.0000001f,
            "Didn't match the offset at the first update");

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.FloatEquals(0.01f, (float)UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            0.0000001f,
            "Didn't match the offset at the first fixed update");

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(0f, (float)UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            0.0000001f,
            "Didn't match the offset at the second update");

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(0.01f, (float)UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            0.0000001f,
            "Didn't match the offset at the third update");

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.FloatEquals(0.01f, (float)UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            0.0000001f,
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

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f, (float)UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            0.0000001f);

        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f, (float)UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            0.0000001f);

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f * 2f % Time.fixedDeltaTime,
            (float)UpdateInvokeOffset.Offset % Time.fixedDeltaTime, 0.0000001f);

        yield return new WaitForFixedUpdateActual();

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f * 3f % Time.fixedDeltaTime,
            (float)UpdateInvokeOffset.Offset % Time.fixedDeltaTime, 0.0000001f);

        yield return new WaitForFixedUpdateActual();

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f * 4f % Time.fixedDeltaTime,
            (float)UpdateInvokeOffset.Offset % Time.fixedDeltaTime, 0.0000001f);

        yield return new WaitForFixedUpdateActual();

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f * 5f % Time.fixedDeltaTime,
            (float)UpdateInvokeOffset.Offset % Time.fixedDeltaTime, 0.0000001f);

        yield return new WaitForFixedUpdateActual();

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f * 6f % Time.fixedDeltaTime,
            (float)UpdateInvokeOffset.Offset % Time.fixedDeltaTime, 0.0000001f);

        yield return new WaitForUpdateActual();

        RuntimeAssert.FloatEquals(1f / 60f * 7f % Time.fixedDeltaTime,
            (float)UpdateInvokeOffset.Offset % Time.fixedDeltaTime, 0.0000001f);

        yield return new WaitForCoroutine(CleanupTest());
    }

    private double _originalFt;
    private float _originalFixedDt;
    private float _originalMaxDt;

    private IEnumerable<CoroutineWait> InitTest(float ft)
    {
        // for testing the occasional double fixed update invokes
        var waitManual = new WaitManual();

        void WaitManualCallback(DateTime dateTime, bool preMonoBehaviourResume)
        {
            if (!preMonoBehaviourResume)
            {
                waitManual.RunNext();
            }
        }

        _gameRestart.OnGameRestartResume += WaitManualCallback;

        _originalFt = _timeEnv.FrameTime;
        _originalFixedDt = Time.fixedDeltaTime;
        _originalMaxDt = Time.maximumDeltaTime;

        _timeEnv.FrameTime = ft;
        Time.fixedDeltaTime = 0.02f;
        Time.maximumDeltaTime = 1f / 3f;

        yield return new WaitForUpdateUnconditional();

        _gameRestart.SoftRestart(DateTime.Now);

        // wait for soft restart
        yield return waitManual;
        _gameRestart.OnGameRestartResume -= WaitManualCallback;
    }

    private IEnumerable<CoroutineWait> CleanupTest()
    {
        _timeEnv.FrameTime = _originalFt;
        Time.fixedDeltaTime = _originalFixedDt;
        Time.maximumDeltaTime = _originalMaxDt;

        yield return new WaitForUpdateActual();
    }
}