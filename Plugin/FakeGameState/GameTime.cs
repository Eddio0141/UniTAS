namespace UniTASPlugin.FakeGameState;

internal static class GameTime
{
    public static System.DateTime Time { get; set; } = System.DateTime.MinValue;
    public static ulong FrameCount { get; set; } = 0;

    public static long Seed()
    {
        return Time.Ticks;
    }
}