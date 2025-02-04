using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment;

[Singleton(RegisterPriority.TimeEnv)]
[ExcludeRegisterIfTesting]
public class TimeEnv : ITimeEnv, IOnGameRestartResume, IOnLastUpdateActual,
    IOnFixedUpdateActual, IOnUpdateUnconditional, IOnStartUnconditional, IOnAwakeUnconditional
{
    private readonly ITimeWrapper _timeWrap;
    private readonly IPatchReverseInvoker _patchReverseInvoker;
    private readonly IMonoBehaviourController _monoBehaviourController;

    public TimeEnv(ITimeWrapper timeWrap, IPatchReverseInvoker patchReverseInvoker,
        IMonoBehaviourController monoBehaviourController)
    {
        _timeWrap = timeWrap;
        _patchReverseInvoker = patchReverseInvoker;
        _monoBehaviourController = monoBehaviourController;

        // start time to current time
        StartupTime = patchReverseInvoker.Invoke(() => DateTime.Now);
        TimeTolerance = _timeWrap.IntFPSOnly ? 1.0 / int.MaxValue : float.Epsilon;
    }

    private const double DefaultFt = 0.01f;

    private bool _setInitialFt;

    public void AwakeUnconditional()
    {
        if (_setInitialFt) return;
        _setInitialFt = true;

        FrameTime = DefaultFt;
    }

    public void StartUnconditional()
    {
        TimeInit();
    }

    private bool _initialTimeSet;

    public void UpdateUnconditional()
    {
        if (_monoBehaviourController.PausedExecution)
        {
            FrameCountRestartOffset++;
            RenderedFrameCountOffset++;
        }

        if (_initialTimeSet) return;
        _initialTimeSet = true;

        // stupid but slightly fixes accuracy on game first start
        RealtimeSinceStartup += DefaultFt;
        UnscaledTime += DefaultFt;
        ScaledTime += DefaultFt * Time.timeScale;
        SecondsSinceStartUp += DefaultFt;
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

    public DateTime CurrentTime => StartupTime + TimeSpan.FromSeconds(RealtimeSinceStartup);
    public ulong RenderedFrameCountOffset { get; private set; }
    public ulong FrameCountRestartOffset { get; private set; }
    public double SecondsSinceStartUp { get; private set; }
    public double UnscaledTime { get; private set; }
    public double FixedUnscaledTime { get; private set; }
    public double ScaledTime { get; private set; }
    public double ScaledFixedTime { get; private set; }
    public double RealtimeSinceStartup { get; private set; }

    private bool _timeInitialized = true;

    public void OnLastUpdateActual()
    {
        var ft = FrameTime;
        RealtimeSinceStartup += ft;
        UnscaledTime += ft;
        ScaledTime += ft * Time.timeScale;
        SecondsSinceStartUp += ft;
    }

    public void FixedUpdateActual()
    {
        TimeInit();

        var fixedDt = Time.fixedDeltaTime;

        var newFixedUnscaledTime = FixedUnscaledTime + fixedDt;
        if (newFixedUnscaledTime <= UnscaledTime)
        {
            FixedUnscaledTime = newFixedUnscaledTime;
        }

        var newScaledFixedTime = ScaledFixedTime + fixedDt * Time.timeScale;
        if (newScaledFixedTime <= ScaledTime)
        {
            ScaledFixedTime = newScaledFixedTime;
        }
    }

    // setting the start up time causes the game to update other time related variables, which requires this to be ran in the main thread
    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (!preMonoBehaviourResume) return;

        StartupTime = startupTime;

        RenderedFrameCountOffset = _patchReverseInvoker.Invoke(() => (ulong)Time.renderedFrameCount);
        SecondsSinceStartUp = 0;
        FrameCountRestartOffset = _patchReverseInvoker.Invoke(() => (ulong)Time.frameCount);
        FixedUnscaledTime = 0;
        UnscaledTime = 0;
        ScaledTime = 0;
        ScaledFixedTime = 0;
        RealtimeSinceStartup = 0;

        _timeInitialized = false;
    }

    private void TimeInit()
    {
        if (_timeInitialized) return;
        _timeInitialized = true;

        FrameCountRestartOffset--;
        RenderedFrameCountOffset--;
        StaticLogger.Trace("initialized time");
    }
}