using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.VersionSafeWrapper;

namespace UniTASPlugin.GameEnvironment;

/// <summary>
///     Helper for apply game environment settings that doesn't apply on it's own.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class VirtualEnvironmentApplier : IOnUpdate
{
    private readonly IVirtualEnvironmentService _virtualEnvironmentService;

    public VirtualEnvironmentApplier(IVirtualEnvironmentService virtualEnvironmentService)
    {
        _virtualEnvironmentService = virtualEnvironmentService;
    }

    public void Update(float deltaTime)
    {
        var env = _virtualEnvironmentService.GetVirtualEnv();
        if (!env.RunVirtualEnvironment) return;

        // frameTime
        if (TimeWrap.CaptureDeltaTimeExists)
        {
            TimeWrap.CaptureFrameTime = env.FrameTime;
        }
        else
        {
            // is it a round number?
            var fps = 1f / env.FrameTime;
            if (fps == (int)fps)
            {
                TimeWrap.CaptureFrameTime = env.FrameTime;
            }
            else
            {
                // warn user
                Plugin.Log.LogWarning(
                    "Frame time is not an integer FPS and can't apply accurately, rounding to nearest integer FPS");
                TimeWrap.CaptureFrameTime = 1f / (int)fps;
            }
        }
    }
}