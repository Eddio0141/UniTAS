using System;
using UniTASPlugin.GameEnvironment.InnerState;

namespace UniTASPlugin.Movie.Models.Properties;

public class StartupPropertiesModel
{
    public Os Os { get; }
    public DateTime StartTime { get; }
    public float FrameTime { get; }
    public WindowState WindowState { get; }
}