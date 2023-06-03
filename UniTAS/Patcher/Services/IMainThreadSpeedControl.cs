namespace UniTAS.Patcher.Services;

/// <summary>
/// Used to control the speed of the main thread
/// Only applies when the virtual environment is enabled
/// </summary>
public interface IMainThreadSpeedControl
{
    /// <summary>
    /// The speed multiplier of the main thread
    /// 0 makes the thread run as fast as possible
    /// 1 makes the thread run at 1x speed, 2 at 2x speed, etc
    /// </summary>
    public float SpeedMultiplier { get; set; }
}