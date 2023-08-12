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
    public IEnumerable<CoroutineWait> SoftRestartTiming()
    {
        var waitManual = new WaitManual();

        void WaitManualCallback(DateTime dateTime, bool preMonoBehaviourResume)
        {
            if (!preMonoBehaviourResume)
            {
                waitManual.RunNext();
            }
        }

        _gameRestart.OnGameRestartResume += WaitManualCallback;

        var originalFt = _timeEnv.FrameTime;
        var originalFixedDt = Time.fixedDeltaTime;
        var originalMaxDt = Time.maximumDeltaTime;

        _timeEnv.FrameTime = 0.01f;
        Time.fixedDeltaTime = 0.02f;
        Time.maximumDeltaTime = 1f / 3f;

        yield return new WaitForUpdateUnconditional();

        _gameRestart.SoftRestart(DateTime.Now);

        // wait for soft restart
        yield return waitManual;
        _gameRestart.OnGameRestartResume -= WaitManualCallback;

        // FixedUpdate is the first thing
        yield return new WaitForFixedUpdateActual();

        RuntimeAssert.AreEqual(0f, UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            "Didn't match offset at the first fixed update");

        yield return new WaitForUpdateActual();

        RuntimeAssert.AreEqual(0.01f, _timeEnv.FrameTime % Time.fixedDeltaTime,
            "Didn't match the offset at the first update");

        yield return new WaitForUpdateActual();

        _timeEnv.FrameTime = originalFt;
        Time.fixedDeltaTime = originalFixedDt;
        Time.maximumDeltaTime = originalMaxDt;
    }
}