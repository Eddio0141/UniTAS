namespace UniTASPlugin.UnitySafeWrappers.Interfaces;

public interface ITimeWrapper
{
    float CaptureFrameTime { get; set; }

    float FixedDeltaTime { get; }
    float DeltaTime { get; }

    int FrameCount { get; }
}