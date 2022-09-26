using System;

namespace UniTASPlugin.FakeGameState;

internal static class GameTime
{
    public static DateTime Time { get; private set; } = DateTime.Now;
    private static bool gotInitialTime = false;
    public static bool GotInitialTime { get => gotInitialTime; set => gotInitialTime = true; }
    public static ulong RenderedFrameCount { get; private set; } = 0;
    public static ulong FrameCountRestartOffset { get; private set; } = 0;
    public static double SecondsSinceStartUp { get; private set; } = 0;

    public static void ResetState(DateTime time)
    {
        Time = time;
        RenderedFrameCount = 0;
        SecondsSinceStartUp = 0;
        FrameCountRestartOffset = (ulong)UnityEngine.Time.frameCount - 1;
    }

    public static void SetState(DateTime time, ulong renderedFrameCount, double secondsSinceStartup, ulong frameCountRestartOffset)
    {
        Time = time;
        RenderedFrameCount = renderedFrameCount;
        SecondsSinceStartUp = secondsSinceStartup;
        FrameCountRestartOffset = frameCountRestartOffset;
    }

    public static void Update(float deltaTime)
    {
        Time += TimeSpan.FromSeconds(deltaTime);
        SecondsSinceStartUp += deltaTime;
        RenderedFrameCount++;
    }

    public static long Seed()
    {
        return Time.Ticks;
    }
}