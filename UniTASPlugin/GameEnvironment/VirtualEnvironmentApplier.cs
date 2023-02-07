using UniTASPlugin.Interfaces.Update;
using UniTASPlugin.UnitySafeWrappers.Interfaces;

namespace UniTASPlugin.GameEnvironment;

/// <summary>
/// Helper for apply game environment settings that doesn't apply on it's own.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class VirtualEnvironmentApplier : IOnPreUpdates
{
    private readonly VirtualEnvironment _virtualEnvironment;
    private readonly ITimeWrapper _timeWrap;

    private float _lastFrameTime;

    public VirtualEnvironmentApplier(VirtualEnvironment virtualEnvironment, ITimeWrapper timeWrap)
    {
        _virtualEnvironment = virtualEnvironment;
        _timeWrap = timeWrap;
    }

    public void PreUpdate()
    {
        if (!_virtualEnvironment.RunVirtualEnvironment)
        {
            _lastFrameTime = -1f;
            return;
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_lastFrameTime != _virtualEnvironment.FrameTime)
        {
            _lastFrameTime = _virtualEnvironment.FrameTime;
            // frameTime
            _timeWrap.CaptureFrameTime = _virtualEnvironment.FrameTime;
        }
    }
}