using System;

namespace UniTAS.Patcher.Services.VirtualEnvironment;

public interface ITimeEnv
{
    double FrameTime { get; set; }
    DateTime CurrentTime { get; }
    ulong RenderedFrameCountOffset { get; }
    ulong FrameCountRestartOffset { get; }
    double SecondsSinceStartUp { get; }
    double UnscaledTime { get; }
    double FixedUnscaledTime { get; }
    double ScaledTime { get; }
    double ScaledFixedTime { get; }
    double RealtimeSinceStartup { get; }
}