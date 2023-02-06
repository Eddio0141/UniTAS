using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.LegacySafeWrappers;

namespace UniTASPlugin.GameEnvironment;

/// <summary>
/// Helper for apply game environment settings that doesn't apply on it's own.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class VirtualEnvironmentApplier : IOnPreUpdates
{
    private readonly IVirtualEnvironmentFactory _virtualEnvironmentFactory;
    private float _lastFrameTime;

    public VirtualEnvironmentApplier(IVirtualEnvironmentFactory virtualEnvironmentFactory)
    {
        _virtualEnvironmentFactory = virtualEnvironmentFactory;
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
        if (!env.RunVirtualEnvironment)
        {
            _lastFrameTime = -1f;
            return;
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_lastFrameTime != env.FrameTime)
        {
            _lastFrameTime = env.FrameTime;
            // frameTime
            TimeWrap.CaptureFrameTime = env.FrameTime;
        }
    }
}