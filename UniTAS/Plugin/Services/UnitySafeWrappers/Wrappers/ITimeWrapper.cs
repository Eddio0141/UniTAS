namespace UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;

public interface ITimeWrapper
{
    /// <summary>
    /// Sets the game's frame time between updates
    /// If IntFPSOnly is true, this will be rounded to the nearest integer
    /// </summary>
    double CaptureFrameTime { get; set; }

    /// <summary>
    /// Returns if frame time can only be FPS values of an integer
    /// </summary>
    bool IntFPSOnly { get; }
}