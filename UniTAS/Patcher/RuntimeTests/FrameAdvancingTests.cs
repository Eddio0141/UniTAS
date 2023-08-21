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
        StaticLogger.Log.LogDebug("Init call");
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
        StaticLogger.Log.LogDebug("CleanUp call");
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

        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForFixedUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();
        yield return new WaitForOnSync();

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

    // [RuntimeTest]
    public IEnumerable<CoroutineWait> FrameAdvanceUpdate()
    {
        yield return new WaitForCoroutine(Init(0.01, 0.02f));

        // pause before update for offset 0 happens
        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update);

        yield return new WaitForUpdateUnconditional();

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update);

        yield return new WaitForUpdateActual();
        // at this point offset should be prev + ft

        RuntimeAssert.AreEqual(0f, UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            "Offset is wrong after frame advance");

        yield return new WaitForCoroutine(CleanUp());
    }

    // [RuntimeTest]
    // public void FrameAdvanceFixedUpdate()
    // {
    // }
    //
    // [RuntimeTest]
    // public void FrameAdvanceUpdateAndFixedUpdate()
    // {
    // }
    //
    // [RuntimeTest]
    // public void MaximumDeltaTimeTest()
    // {
    // }
}