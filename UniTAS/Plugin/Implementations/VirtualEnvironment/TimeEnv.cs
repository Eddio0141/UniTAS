using System;
using System.Diagnostics;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.VirtualEnvironment;

[Singleton]
public class TimeEnv : ITimeEnv, IOnPreUpdatesUnconditional, IOnGameRestartResume, IOnStartUnconditional,
    IOnLastUpdateUnconditional, IOnFixedUpdateUnconditional, IOnUpdateUnconditional
{
    private readonly IConfig _config;

    private readonly ITimeWrapper _timeWrap;

    public TimeEnv(IConfig config, ITimeWrapper timeWrap)
    {
        _config = config;
        _timeWrap = timeWrap;
        FrameTime = 0f;
    }

    public double FrameTime
    {
        get => _timeWrap.CaptureFrameTime;
        set
        {
            if (value <= 0) value = 1f / _config.DefaultFps;
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
    private bool _initialUpdate = true;

    public void PreUpdateUnconditional()
    {
        TimeInit();
    }

    public void UpdateUnconditional()
    {
        if (!_initialUpdate) return;
        _initialUpdate = false;

        RealtimeSinceStartup += FrameTime;
        UnscaledTime += FrameTime;
        ScaledTime += Time.deltaTime;
        SecondsSinceStartUp += FrameTime;
    }

    // TODO use pause update
    public void OnLastUpdateUnconditional()
    {
        RealtimeSinceStartup += FrameTime;
        UnscaledTime += FrameTime;
        ScaledTime += Time.deltaTime;
        SecondsSinceStartUp += FrameTime;
    }

    public void FixedUpdateUnconditional()
    {
        var fixedDt = Time.fixedDeltaTime;

        var newFixedUnscaledTime = FixedUnscaledTime + fixedDt;
        if (newFixedUnscaledTime <= UnscaledTime)
        {
            FixedUnscaledTime += fixedDt;
        }

        var newScaledFixedTime = ScaledFixedTime + fixedDt;
        if (newScaledFixedTime <= ScaledTime)
        {
            ScaledFixedTime += fixedDt;
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

        Trace.Write($"Setting startup time to {StartupTime}");
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
        Trace.Write($"New game time state: {this}");

        _timeInitialized = false;
        _initialUpdate = true;
    }

    public void StartUnconditional()
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