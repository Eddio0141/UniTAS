using System;
using HarmonyLib;
using UnityEngine;

namespace UniTASPlugin.FakeGameState;

internal static class GameTime
{
    public static DateTime StartupTime { get; private set; } = new(2000, 1, 1);
    public static DateTime CurrentTime => StartupTime + TimeSpan.FromSeconds(RealtimeSinceStartup);
    public static ulong RenderedFrameCountOffset { get; private set; }
    public static ulong FrameCountRestartOffset { get; private set; }
    public static double SecondsSinceStartUpOffset { get; private set; }
    public static double UnscaledTimeOffset { get; private set; }
    public static double FixedUnscaledTimeOffset { get; private set; }
    public static double ScaledTimeOffset { get; private set; }
    public static double ScaledFixedTimeOffset { get; private set; }
    public static float RealtimeSinceStartup { get; private set; } = Time.realtimeSinceStartup;

    public static void ResetState(DateTime time)
    {
        StartupTime = time;
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

    public static void Update()
    {
        RealtimeSinceStartup = Time.realtimeSinceStartup;
    }

    /*
    public static void SetState(DateTime time, ulong renderedFrameCount, double secondsSinceStartup, double secondsSinceStartUpTimeScale, ulong frameCountRestartOffset)
    {
        Time = time;
        RenderedFrameCount = renderedFrameCount;
        SecondsSinceStartUp = secondsSinceStartup;
        SecondsSinceStartUpTimeScale = secondsSinceStartUpTimeScale;
        FrameCountRestartOffset = frameCountRestartOffset;
    }
    */

    public static long Seed()
    {
        return CurrentTime.Ticks;
    }
}