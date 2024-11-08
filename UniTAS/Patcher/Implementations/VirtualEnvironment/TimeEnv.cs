using System;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.DontRunIfPaused;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.VirtualEnvironment;

[Singleton(RegisterPriority.TimeEnv)]
[ExcludeRegisterIfTesting]
public class TimeEnv : ITimeEnv, IOnPreUpdateActual, IOnGameRestartResume, IOnStartActual, IOnLastUpdateActual,
    IOnFixedUpdateActual, IOnUpdateUnconditional, IOnStartUnconditional
{
    private readonly ITimeWrapper _timeWrap;

    public TimeEnv(ITimeWrapper timeWrap, IPatchReverseInvoker patchReverseInvoker)
    {
        _timeWrap = timeWrap;

        // start time to current time
        StartupTime = patchReverseInvoker.Invoke(() => DateTime.Now);
        TimeTolerance = _timeWrap.IntFPSOnly ? 1.0 / int.MaxValue : float.Epsilon;
    }

    private const double DefaultFt = 0.01f;

    private bool _setInitialFt;

    public void StartUnconditional()
    {
        if (_setInitialFt) return;
        _setInitialFt = true;

        FrameTime = DefaultFt;
    }

    private bool _initialTimeSet;

    public void UpdateUnconditional()
    {
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

    private bool _timeInitialized;

    public void PreUpdateActual()
    {
        TimeInit();
    }

    public void OnLastUpdateActual()
    {
        RealtimeSinceStartup += FrameTime;
        UnscaledTime += FrameTime;
        ScaledTime += FrameTime * Time.timeScale;
        SecondsSinceStartUp += FrameTime;
    }

    public void FixedUpdateActual()
    {
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

    public override string ToString()
    {
        return $"StartupTime: {StartupTime} " +
               $"CurrentTime: {CurrentTime} " +
               $"RenderedFrameCountOffset: {RenderedFrameCountOffset} " +
               $"SecondsSinceStartUp: {SecondsSinceStartUp} " +
               $"FrameCountRestartOffset: {FrameCountRestartOffset} " +
               $"FixedUnscaledTime: {FixedUnscaledTime} " +
               $"UnscaledTime: {UnscaledTime} " +
               $"ScaledTime: {ScaledTime} " +
               $"ScaledFixedTime: {ScaledFixedTime} " +
               $"RealtimeSinceStartup: {RealtimeSinceStartup}";
    }

    // setting the start up time causes the game to update other time related variables, which requires this to be ran in the main thread
    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (!preMonoBehaviourResume) return;

        StartupTime = startupTime;

        RenderedFrameCountOffset += (ulong)Time.renderedFrameCount;
        SecondsSinceStartUp = 0;
        FrameCountRestartOffset += (ulong)Time.frameCount;
        var fixedUnscaledTime = AccessTools.Property(typeof(Time), "fixedUnscaledTime");
        if (fixedUnscaledTime != null)
            FixedUnscaledTime = 0;
        var unscaledTime = AccessTools.Property(typeof(Time), "unscaledTime");
        if (unscaledTime != null)
            UnscaledTime = 0;
        ScaledTime = 0;
        ScaledFixedTime = 0;
        RealtimeSinceStartup = 0;

        // funny hack to update the time properly but also with starting time
        var initialFt = _timeWrap.CaptureFrameTime;
        RealtimeSinceStartup += initialFt;
        UnscaledTime += initialFt;
        ScaledTime += initialFt;
        SecondsSinceStartUp += initialFt;

        _timeInitialized = false;
    }

    public void StartActual()
    {
        TimeInit();
    }

    private void TimeInit()
    {
        if (_timeInitialized) return;
        _timeInitialized = true;

        FrameCountRestartOffset--;
        RenderedFrameCountOffset--;
    }
}