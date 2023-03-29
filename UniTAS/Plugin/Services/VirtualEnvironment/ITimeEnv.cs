using System;

namespace UniTAS.Plugin.Services.VirtualEnvironment;

public interface ITimeEnv
{
    float FrameTime { get; set; }
    DateTime CurrentTime { get; }
    ulong RenderedFrameCountOffset { get; }
    ulong FrameCountRestartOffset { get; }
    double SecondsSinceStartUp { get; }
    double UnscaledTime { get; }
    double FixedUnscaledTime { get; }
    double ScaledTime { get; }
    double ScaledFixedTime { get; }
    float RealtimeSinceStartup { get; }
}