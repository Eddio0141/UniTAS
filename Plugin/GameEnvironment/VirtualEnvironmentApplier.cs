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

        if (deltaTime == 0)
        {
            TimeWrap.CaptureFrameTime = 0;
        }

        // frameTime
        TimeWrap.CaptureFrameTime = env.FrameTime;

        if (!TimeWrap.CaptureDeltaTimeExists)
        {
            // is it a round number?
            var fps = 1f / env.FrameTime;
            if (fps != (int)fps)
            {
                // warn user
                Plugin.Log.LogWarning(
                    "Frame time is not an integer FPS and can't apply accurately, rounding to nearest integer FPS");
            }
        }
    }
}