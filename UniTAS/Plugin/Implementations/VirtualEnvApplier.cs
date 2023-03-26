using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents;
using UniTAS.Plugin.Services.UnitySafeWrappers.Wrappers;
using UniTAS.Plugin.Services.VirtualEnvironment;

namespace UniTAS.Plugin.Implementations;

/// <summary>
/// Helper for apply game environment settings that doesn't apply on it's own.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
[Singleton]
public class VirtualEnvApplier : IOnPreUpdates
{
    private readonly ITimeEnv _timeEnv;
    private readonly ITimeWrapper _timeWrap;

    private float _lastFrameTime;

    public VirtualEnvApplier(ITimeEnv timeEnv, ITimeWrapper timeWrap)
    {
        _timeEnv = timeEnv;
        _timeWrap = timeWrap;
    }

    public void PreUpdate()
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_lastFrameTime != _timeEnv.FrameTime)
        {
            _lastFrameTime = _timeEnv.FrameTime;
            // frameTime
            _timeWrap.CaptureFrameTime = _timeEnv.FrameTime;
        }
    }
}