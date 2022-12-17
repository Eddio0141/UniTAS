using System;
using HarmonyLib;
using UniTASPlugin.Interfaces.Update;
using UnityEngine;

namespace UniTASPlugin.GameEnvironment.InnerState;

// ReSharper disable once ClassNeverInstantiated.Global
public class GameTime : IOnUpdate
{
    private DateTime _startupTime = new(2000, 1, 1);

    /// <summary>
    /// Setting the start up time causes the game to update other time related variables, which requires this to be ran in the main thread.
    /// </summary>
    public DateTime StartupTime
    {
        get => _startupTime;
        set
        {
            _startupTime = value;
            RenderedFrameCountOffset += (ulong)Time.renderedFrameCount;
            SecondsSinceStartUpOffset += Time.realtimeSinceStartup;
            FrameCountRestartOffset += (ulong)Time.frameCount - 1;
            var fixedUnscaledTime = Traverse.Create(typeof(Time)).Property("fixedUnscaledTime");
            if (fixedUnscaledTime.PropertyExists())
                FixedUnscaledTimeOffset += fixedUnscaledTime.GetValue<float>();
            var unscaledTime = Traverse.Create(typeof(Time)).Property("unscaledTime");
            if (unscaledTime.PropertyExists())
                UnscaledTimeOffset += unscaledTime.GetValue<float>();
            ScaledTimeOffset += Time.time;
            ScaledFixedTimeOffset += Time.fixedTime;
        }
    }

    public DateTime CurrentTime => StartupTime + TimeSpan.FromSeconds(RealtimeSinceStartup);
    public ulong RenderedFrameCountOffset { get; private set; }
    public ulong FrameCountRestartOffset { get; private set; }
    public double SecondsSinceStartUpOffset { get; private set; }
    public double UnscaledTimeOffset { get; private set; }
    public double FixedUnscaledTimeOffset { get; private set; }
    public double ScaledTimeOffset { get; private set; }
    public double ScaledFixedTimeOffset { get; private set; }
    public float RealtimeSinceStartup { get; private set; }

    public void Update(float deltaTime)
    {
        RealtimeSinceStartup = Time.realtimeSinceStartup;
    }
}