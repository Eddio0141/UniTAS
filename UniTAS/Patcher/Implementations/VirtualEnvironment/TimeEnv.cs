using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Events.UnityEvents;
using UniTAS.Patcher.Models.EventSubscribers;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.UnityEvents;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment;

[Singleton]
[ExcludeRegisterIfTesting]
public class TimeEnv : ITimeEnv, IOnGameRestartResume, IOnSceneLoad
{
    private readonly ITimeWrapper _timeWrap;
    private readonly IPatchReverseInvoker _patchReverseInvoker;
    private readonly IUpdateEvents _updateEvents;

    public TimeEnv(ITimeWrapper timeWrap, IPatchReverseInvoker patchReverseInvoker,
        IMonoBehaviourController monoBehaviourController, IUpdateEvents updateEvents)
    {
        _timeWrap = timeWrap;
        _patchReverseInvoker = patchReverseInvoker;
        _updateEvents = updateEvents;

        // start time to current time
        StartupTime = patchReverseInvoker.Invoke(() => DateTime.Now);
        TimeTolerance = _timeWrap.IntFPSOnly ? 1.0 / int.MaxValue : float.Epsilon;

        // once only
        _updateEvents.OnAwakeUnconditional += AwakeUnconditional;
        _updateEvents.OnStartUnconditional += StartOnce;

        _updateEvents.AddPriorityCallback(CallbackUpdate.FixedUpdateActual, FixedUpdateActual,
            CallbackPriority.TimeEnv);
        _updateEvents.AddPriorityCallback(CallbackUpdate.LastUpdateActual, OnLastUpdateActual,
            CallbackPriority.TimeEnv);

        monoBehaviourController.OnPauseChange += pause =>
        {
            if (pause)
            {
                // must be added to be ran after FirstUpdateSkipOnRestart UpdateUnconditional
                _updateEvents.OnUpdateUnconditional += UpdateDuringPause;
            }
            else
            {
                _updateEvents.OnUpdateUnconditional -= UpdateDuringPause;
            }
        };
    }

    private const double DefaultFt = 0.01f;

    private void AwakeUnconditional()
    {
        _updateEvents.OnAwakeUnconditional -= AwakeUnconditional;
        FrameTime = DefaultFt;
    }

    private void UpdateDuringPause()
    {
        FrameCountRestartOffset++;
        RenderedFrameCountOffset++;
    }

    private void StartOnce()
    {
        _updateEvents.OnStartUnconditional -= StartOnce;

        // stupid but slightly fixes accuracy on game first start
        var scaledFt = DefaultFt * Time.timeScale;
        UnscaledTime += DefaultFt;
        ScaledTime += scaledFt;
        TimeSinceLevelLoad += scaledFt;
    }

    public double TimeTolerance { get; }

    public double FrameTime
    {
        get => _timeWrap.CaptureFrameTime;
        set
        {
            if (value <= 0f) value = DefaultFt;
            _timeWrap.CaptureFrameTime = value;
        }
    }

    private DateTime StartupTime { get; set; }

    public DateTime CurrentTime => StartupTime + TimeSpan.FromSeconds(UnscaledTime);
    public ulong RenderedFrameCountOffset { get; private set; }
    public ulong FrameCountRestartOffset { get; private set; }
    public double UnscaledTime { get; private set; }
    public double FixedUnscaledTime { get; private set; }
    public double ScaledTime { get; private set; }
    public double ScaledFixedTime { get; private set; }
    public double TimeSinceLevelLoad { get; private set; }
    public double FixedTimeSinceLevelLoad { get; private set; }

    private void OnLastUpdateActual()
    {
        var ft = FrameTime;
        var scaledFt = ft * Time.timeScale;
        UnscaledTime += ft;
        ScaledTime += scaledFt;
        TimeSinceLevelLoad += scaledFt;
    }

    private void FixedUpdateActual()
    {
        var fixedDt = Time.fixedDeltaTime;
        var scaledFixedDt = fixedDt * Time.timeScale;

        var newFixedUnscaledTime = FixedUnscaledTime + fixedDt;
        if (newFixedUnscaledTime <= UnscaledTime)
        {
            FixedUnscaledTime = newFixedUnscaledTime;
        }

        var newScaledFixedTime = ScaledFixedTime + scaledFixedDt;
        if (newScaledFixedTime <= ScaledTime)
        {
            ScaledFixedTime = newScaledFixedTime;
        }

        var newFixedTimeSinceLevelLoad = FixedTimeSinceLevelLoad + scaledFixedDt;
        if (newFixedTimeSinceLevelLoad <= TimeSinceLevelLoad)
            FixedTimeSinceLevelLoad = newFixedUnscaledTime;
    }

    // setting the start up time causes the game to update other time related variables, which requires this to be ran in the main thread
    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (!preMonoBehaviourResume) return;

        StartupTime = startupTime;

        RenderedFrameCountOffset = _patchReverseInvoker.Invoke(() => (ulong)Time.renderedFrameCount);
        FrameCountRestartOffset = _patchReverseInvoker.Invoke(() => (ulong)Time.frameCount);
        FixedUnscaledTime = 0;
        UnscaledTime = 0;
        ScaledTime = 0;
        ScaledFixedTime = 0;
        TimeSinceLevelLoad = 0;
        FixedTimeSinceLevelLoad = 0;

        _updateEvents.AddPriorityCallback(CallbackUpdate.StartUnconditional, TimeInit, CallbackPriority.TimeEnv);
        _updateEvents.AddPriorityCallback(CallbackUpdate.FixedUpdateActual, TimeInit, CallbackPriority.TimeEnv);
    }

    private void TimeInit()
    {
        _updateEvents.OnStartUnconditional -= TimeInit;
        _updateEvents.OnFixedUpdateActual -= TimeInit;

        FrameCountRestartOffset--;
        RenderedFrameCountOffset--;

        var ft = FrameTime;
        var scaledFt = ft * Time.timeScale;
        UnscaledTime += ft;
        ScaledTime += scaledFt;
        TimeSinceLevelLoad += scaledFt;

        StaticLogger.Trace("initialized time");
    }

    public void OnSceneLoad()
    {
        TimeSinceLevelLoad = 0f;
        FixedTimeSinceLevelLoad = 0f;
    }
}