using System.Collections;
using UniTAS.Patcher.Implementations.Coroutine;
using UniTAS.Patcher.Implementations.FrameAdvancing;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.RuntimeTest;
using UniTAS.Patcher.Services.EventSubscribers;
using UniTAS.Patcher.Services.FrameAdvancing;
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

    public FrameAdvancingTests(ITimeEnv timeEnv, IFrameAdvancing frameAdvancing, IUpdateEvents updateEvents)
    {
        _timeEnv = timeEnv;
        _frameAdvancing = frameAdvancing;
        _updateEvents = updateEvents;
    }

    [RuntimeTest]
    public IEnumerable FrameAdvanceFirstPause()
    {
        var restoreFt = _timeEnv.FrameTime;
        var restoreFixedDt = Time.fixedDeltaTime;
        var restoreMaxDt = Time.maximumDeltaTime;

        var actualUpdateCounter = 0;

        // test with 100 fps and 0.02s fixed dt
        _timeEnv.FrameTime = 0.01;
        Time.fixedDeltaTime = 0.02f;

        yield return new WaitForUpdateUnconditional();
        yield return new WaitForOnSync();

        // is it paused?
        // this should pause it instantly
        _frameAdvancing.FrameAdvance(2, FrameAdvanceMode.Update);
        _updateEvents.OnUpdateActual += () => { actualUpdateCounter++; };

        yield return new WaitForUpdateUnconditional();
        yield return new WaitForUpdateUnconditional();

        RuntimeAssert.AreEqual(0, actualUpdateCounter, "Update counter should be 0 since frame advance is activated");

        // clean up
        _frameAdvancing.TogglePause();
        _timeEnv.FrameTime = restoreFt;
        Time.fixedDeltaTime = restoreFixedDt;
        Time.maximumDeltaTime = restoreMaxDt;
        yield return new WaitForUpdateUnconditional();
    }

    [RuntimeTest]
    public IEnumerable FrameAdvanceUpdate()
    {
        var restoreFt = _timeEnv.FrameTime;
        var restoreFixedDt = Time.fixedDeltaTime;
        var restoreMaxDt = Time.maximumDeltaTime;

        // test with 100 fps and 0.02s fixed dt
        _timeEnv.FrameTime = 0.01;
        Time.fixedDeltaTime = 0.02f;

        yield return new WaitForUpdateUnconditional();
        yield return new WaitForOnSync();

        // pause before update for offset 0 happens
        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update);

        yield return new WaitForUpdateUnconditional();

        _frameAdvancing.FrameAdvance(1, FrameAdvanceMode.Update);

        yield return new WaitForUpdateActual();
        // at this point offset should be prev + ft

        RuntimeAssert.AreEqual(0f, UpdateInvokeOffset.Offset % Time.fixedDeltaTime,
            "Offset is wrong after frame advance");

        // clean up
        _frameAdvancing.TogglePause();
        _timeEnv.FrameTime = restoreFt;
        Time.fixedDeltaTime = restoreFixedDt;
        Time.maximumDeltaTime = restoreMaxDt;
        yield return new WaitForUpdateUnconditional();
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