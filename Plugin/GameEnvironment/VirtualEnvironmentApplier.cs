using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.LegacySafeWrappers;
using UniTASPlugin.Logger;

namespace UniTASPlugin.GameEnvironment;

/// <summary>
/// Helper for apply game environment settings that doesn't apply on it's own.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class VirtualEnvironmentApplier : IOnPreUpdates
{
    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;
    private readonly ILogger _logger;

    public VirtualEnvironmentApplier(IVirtualEnvironmentFactory virtualEnvironmentFactory, ILogger logger)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
        _logger = logger;
    }

    public void PreUpdate()
    {
        ApplyEnv();
        UpdateState();
    }

    private void UpdateState()
    {
        var env = _virtualEnvironmentFactory.GetVirtualEnv();
        env.InputState.Update();
    }

    private void ApplyEnv()
    {
        var env = _virtualEnvironmentFactory.GetVirtualEnv();
        if (!env.RunVirtualEnvironment) return;

        // frameTime
        TimeWrap.CaptureFrameTime = env.FrameTime;

        // if (!TimeWrap.CaptureDeltaTimeExists)
        // {
        //     // is it a round number?
        //     var fps = 1f / env.FrameTime;
        //     if (Math.Abs(fps - (int)fps) > 0.0001)
        //     {
        //         // warn user
        //         _logger.LogWarning(
        //             "Frame time is not an integer FPS and can't apply accurately, rounding to nearest integer FPS");
        //     }
        // }
    }
}