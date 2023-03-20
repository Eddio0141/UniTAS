using System;
using System.Diagnostics;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UnityEngine;

namespace UniTAS.Plugin.GameEnvironment.InnerState;

// ReSharper disable once ClassNeverInstantiated.Global
public class GameTime : IOnPreUpdates, IOnGameRestartResume, IOnStart
{
    private DateTime StartupTime { get; set; }

    public DateTime CurrentTime => StartupTime + TimeSpan.FromSeconds(RealtimeSinceStartup);
    public ulong RenderedFrameCountOffset { get; private set; }
    public ulong FrameCountRestartOffset { get; private set; }
    public double SecondsSinceStartUpOffset { get; private set; }
    public double UnscaledTimeOffset { get; private set; }
    public double FixedUnscaledTimeOffset { get; private set; }
    public double ScaledTimeOffset { get; private set; }
    public double ScaledFixedTimeOffset { get; private set; }
    public float RealtimeSinceStartup { get; private set; }

    private bool _pendingFrameCountReset;

    public void PreUpdate()
    {
        HandlePendingFrameCountReset();
        RealtimeSinceStartup += Time.deltaTime;
    }

    public override string ToString()
    {
        return $"StartupTime: {StartupTime} " +
               $"CurrentTime: {CurrentTime} " +
               $"RenderedFrameCountOffset: {RenderedFrameCountOffset} " +
               $"SecondsSinceStartUpOffset: {SecondsSinceStartUpOffset} " +
               $"FrameCountRestartOffset: {FrameCountRestartOffset} " +
               $"FixedUnscaledTimeOffset: {FixedUnscaledTimeOffset} " +
               $"UnscaledTimeOffset: {UnscaledTimeOffset} " +
               $"ScaledTimeOffset: {ScaledTimeOffset} " +
               $"ScaledFixedTimeOffset: {ScaledFixedTimeOffset} " +
               $"RealtimeSinceStartup: {RealtimeSinceStartup}";
    }


    // setting the start up time causes the game to update other time related variables, which requires this to be ran in the main thread
    public void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (!preMonoBehaviourResume) return;
        
        StartupTime = startupTime;

        Trace.Write($"Setting startup time to {StartupTime}");
        RenderedFrameCountOffset += (ulong)Time.renderedFrameCount;
        SecondsSinceStartUpOffset += Time.realtimeSinceStartup;
        FrameCountRestartOffset += (ulong)Time.frameCount;
        var fixedUnscaledTime = Traverse.Create(typeof(Time)).Property("fixedUnscaledTime");
        if (fixedUnscaledTime.PropertyExists())
            FixedUnscaledTimeOffset += fixedUnscaledTime.GetValue<float>();
        var unscaledTime = Traverse.Create(typeof(Time)).Property("unscaledTime");
        if (unscaledTime.PropertyExists())
            UnscaledTimeOffset += unscaledTime.GetValue<float>();
        ScaledTimeOffset += Time.time;
        ScaledFixedTimeOffset += Time.fixedTime;
        RealtimeSinceStartup = 0f;
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