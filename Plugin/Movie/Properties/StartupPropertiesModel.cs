using System;
using UniTASPlugin.GameEnvironment;
using OsEnvironment = UniTASPlugin.GameEnvironment.OsEnvironment;

namespace UniTASPlugin.Movie.Properties;

public class StartupPropertiesModel
{
    public OsEnvironment Environment { get; }
    public DateTime StartTime { get; }
    public float FrameTime { get; }
    public WindowState WindowState { get; }
}