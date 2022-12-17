using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.VersionSafeWrapper;

namespace UniTASPlugin.GameEnvironment;

/// <summary>
///     Helper for apply game environment settings that doesn't apply on it's own.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class VirtualEnvironmentApplier : IOnUpdate
{
    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;

    public VirtualEnvironmentApplier(IVirtualEnvironmentFactory virtualEnvironmentFactory)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
    }

    public void Update(float deltaTime)
    {
        var env = _virtualEnvironmentFactory.GetVirtualEnv();
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