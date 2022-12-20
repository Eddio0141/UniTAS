using System;
using UniTASPlugin.GameEnvironment.InnerState;

namespace UniTASPlugin.Movie.MovieModels.Properties;

public class StartupPropertiesModel
{
    public Os Os { get; }
    public DateTime StartTime { get; }
    public float FrameTime { get; }
    public WindowState WindowState { get; }

    public StartupPropertiesModel(Os os, DateTime startTime, float frameTime, WindowState windowState)
    {
        Os = os;
        StartTime = startTime;
        FrameTime = frameTime;
        WindowState = windowState;
    }
}