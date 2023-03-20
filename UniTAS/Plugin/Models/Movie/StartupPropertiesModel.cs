using System;

namespace UniTAS.Plugin.Models.Movie;

public class StartupPropertiesModel
{
    // public Os Os { get; }
    public DateTime StartTime { get; }

    public float FrameTime { get; }
    // public WindowState WindowState { get; }

    public StartupPropertiesModel(DateTime startTime, float frameTime)
    {
        // Os = os;
        StartTime = startTime;
        FrameTime = frameTime;
    }

    public override string ToString()
    {
        return $"StartTime: {StartTime}, FrameTime: {FrameTime}";
    }
}