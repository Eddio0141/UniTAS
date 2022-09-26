using HarmonyLib;
using System;
using UnityEngine;

namespace UniTASPlugin.FakeGameState;

internal static class GameTime
{
    public static DateTime StartupTime { get; private set; } = DateTime.Now;
    public static DateTime CurrentTime { get => StartupTime + TimeSpan.FromSeconds(Time.realtimeSinceStartup); }
    private static bool gotInitialTime = false;
    public static bool GotInitialTime { get => gotInitialTime; set => gotInitialTime = true; }
    public static ulong RenderedFrameCountOffset { get; private set; } = 0;
    public static ulong FrameCountRestartOffset { get; private set; } = 0;
    public static double SecondsSinceStartUpOffset { get; private set; } = 0;
    public static double UnscaledTimeOffset { get; private set; } = 0;
    public static double FixedUnscaledTimeOffset { get; private set; } = 0;
    public static double ScaledTimeOffset { get; private set; } = 0;
    public static double ScaledFixedTimeOffset { get; private set; } = 0;

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