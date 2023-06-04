using System;

namespace UniTAS.Patcher.Models.Movie;

public class StartupPropertiesModel
{
    // public Os Os { get; }
    // public WindowState WindowState { get; }
    public DateTime StartTime { get; }
    public float FrameTime { get; }
    public long Seed { get; }

    public StartupPropertiesModel(DateTime startTime, float frameTime, long seed)
    {
        // Os = os;
        StartTime = startTime;
        FrameTime = frameTime;
        Seed = seed;
    }

    public override string ToString()
    {
        return $"StartTime: {StartTime}, FrameTime: {FrameTime}, Seed: {Seed}";
    }
}