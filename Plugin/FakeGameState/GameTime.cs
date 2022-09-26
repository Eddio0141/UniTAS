using System;

namespace UniTASPlugin.FakeGameState;

internal static class GameTime
{
    public static DateTime Time { get; private set; } = DateTime.Now;
    private static bool gotInitialTime = false;
    public static bool GotInitialTime { get => gotInitialTime; set => gotInitialTime = true; }
    public static ulong FrameCount { get; private set; } = 0;

    public static void ResetState(DateTime time)
    {
        Time = time;
        FrameCount = 0;
    }

    public static void SetState(DateTime time, ulong framecount)
    {
        Time = time;
        FrameCount = framecount;
    }

    public static void Update(float deltaTime)
    {
        Time += TimeSpan.FromSeconds(deltaTime);
        FrameCount++;
    }

    public static long Seed()
    {
        return Time.Ticks;
    }
}