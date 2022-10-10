using System;
using UniTASPlugin.GameEnvironment;
using Environment = UniTASPlugin.GameEnvironment.Environment;

namespace UniTASPlugin.Movie.Model.Properties;

public class StartupProperties
{
    public Environment Environment { get; }
    public DateTime StartTime { get; }
    public float FrameTime { get; }
    public WindowState WindowState { get; }
}