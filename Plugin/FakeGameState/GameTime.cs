using HarmonyLib;
using System;

namespace UniTASPlugin.FakeGameState;

internal static class GameTime
{
    public static DateTime StartupTime { get; private set; } = DateTime.Now;
    public static DateTime CurrentTime { get => StartupTime + TimeSpan.FromSeconds(UnityEngine.Time.realtimeSinceStartup); }
    private static bool gotInitialTime = false;
    public static bool GotInitialTime { get => gotInitialTime; set => gotInitialTime = true; }
    public static ulong RenderedFrameCountOffset { get; private set; } = 0;
    public static ulong FrameCountRestartOffset { get; private set; } = 0;
    public static double SecondsSinceStartUpOffset { get; private set; } = 0;
    public static double FixedUnscaledTimeOffset { get; private set; } = 0;
    static Traverse fixedUnscaledDeltaTime = Traverse.Create(typeof(UnityEngine.Time)).Property("fixedUnscaledDeltaTime");

    public static void ResetState(DateTime time)
    {
        StartupTime = time;
        RenderedFrameCountOffset += (ulong)UnityEngine.Time.renderedFrameCount;
        SecondsSinceStartUpOffset += UnityEngine.Time.realtimeSinceStartup;
        FrameCountRestartOffset += (ulong)UnityEngine.Time.frameCount - 1;
        var fixedUnscaledTime = Traverse.Create(typeof(UnityEngine.Time)).Property("fixedUnscaledTime");
        if (fixedUnscaledTime.PropertyExists())
            FixedUnscaledTimeOffset += fixedUnscaledTime.GetValue<float>();
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