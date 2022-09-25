namespace UniTASPlugin.FakeGameState;

internal static class GameTime
{
    public static System.DateTime Time { get; set; } = System.DateTime.Now;
    private static bool gotInitialTime = false;
    public static bool GotInitialTime { get => gotInitialTime; set => gotInitialTime = true; }
    public static ulong FrameCount { get; set; } = 0;

    public static long Seed()
    {
        return Time.Ticks;
    }
}