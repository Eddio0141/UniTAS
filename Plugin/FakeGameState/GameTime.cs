using System;

namespace UniTASPlugin.FakeGameState;

internal static class GameTime
{
    public static DateTime Time { get; private set; } = DateTime.Now;
    private static bool gotInitialTime = false;
    public static bool GotInitialTime { get => gotInitialTime; set => gotInitialTime = true; }
    public static ulong FrameCount { get; private set; } = 0;
    public static double SecondsSinceStartUp { get; private set; } = 0;

    public static void ResetState(DateTime time)
    {
        Time = time;
        FrameCount = 0;
        SecondsSinceStartUp = 0;
    }

    public static void SetState(DateTime time, ulong framecount, double secondsSinceStartup)
    {
        Time = time;
        FrameCount = framecount;
        SecondsSinceStartUp = secondsSinceStartup;
    }

    public static void Update(float deltaTime)
    {
        Time += TimeSpan.FromSeconds(deltaTime);
        SecondsSinceStartUp += deltaTime;
        FrameCount++;
    }

    public static long Seed()
    {
        return Time.Ticks;
    }
}