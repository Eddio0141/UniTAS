using System;
using System.Diagnostics;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UnityEngine;

namespace UniTAS.Plugin.Services.VirtualEnvironment.InnerState;

// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class GameTime : IOnPreUpdates, IOnGameRestartResume, IOnStart, IOnFixedUpdate
{
    private DateTime StartupTime { get; set; }

    public DateTime CurrentTime => StartupTime + TimeSpan.FromSeconds(RealtimeSinceStartup);
    public ulong RenderedFrameCountOffset { get; private set; }
    public ulong FrameCountRestartOffset { get; private set; }
    public double SecondsSinceStartUp { get; private set; }
    public double UnscaledTime { get; private set; }
    public double FixedUnscaledTime { get; private set; }
    public double ScaledTime { get; private set; }
    public double ScaledFixedTime { get; private set; }
    public float RealtimeSinceStartup { get; private set; }

    private bool _pendingFrameCountReset;

    public void PreUpdate()
    {
        HandlePendingFrameCountReset();

        var scale = Time.timeScale;
        var dt = Time.deltaTime;
        var dtUnscaled = dt / scale;

        RealtimeSinceStartup += dtUnscaled;
        UnscaledTime += dtUnscaled;
        ScaledTime += dt;
        SecondsSinceStartUp += dtUnscaled;
    }

    public void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;

        FixedUnscaledTime += dt;
        ScaledFixedTime += dt;
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

        _pendingFrameCountReset = true;
    }

    public void Start()
    {
        HandlePendingFrameCountReset();
    }

    private void HandlePendingFrameCountReset()
    {
        if (!_pendingFrameCountReset) return;
        _pendingFrameCountReset = false;

        FrameCountRestartOffset--;
        RenderedFrameCountOffset--;
    }
}