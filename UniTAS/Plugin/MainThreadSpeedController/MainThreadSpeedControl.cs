using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UniTAS.Plugin.GameEnvironment;
using UniTAS.Plugin.Interfaces.Update;
using UniTAS.Plugin.ReverseInvoker;
using UnityEngine;

namespace UniTAS.Plugin.MainThreadSpeedController;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class MainThreadSpeedControl : IMainThreadSpeedControl, IOnUpdate
{
    private readonly VirtualEnvironment _virtualEnvironment;
    private readonly IPatchReverseInvoker _patchReverseInvoker;

    public float SpeedMultiplier
    {
        get => _speedMultiplier;
        set
        {
            _speedMultiplier = value < 0f ? 0f : value;
            _lastTime = CurrentTime;
            _remainingTime = 0f;
        }
    }

    private float _lastTime;
    private float _speedMultiplier;

    private float _remainingTime;

    public MainThreadSpeedControl(VirtualEnvironment virtualEnvironment, IPatchReverseInvoker patchReverseInvoker)
    {
        _virtualEnvironment = virtualEnvironment;
        _patchReverseInvoker = patchReverseInvoker;
    }

    private float CurrentTime => _patchReverseInvoker.Invoke(() => Time.realtimeSinceStartup);

    public void Update()
    {
        if (_speedMultiplier == 0 || !_virtualEnvironment.RunVirtualEnvironment ||
            _virtualEnvironment.FrameTime == 0f) return;

        var timeSinceLastUpdate = CurrentTime - _lastTime;
        _lastTime = CurrentTime;

        // if the actual time passed is less than the time that should have passed, wait
        var waitTime = _virtualEnvironment.FrameTime / _speedMultiplier - timeSinceLastUpdate + _remainingTime;
        if (waitTime <= 0)
        {
            return;
        }

        var waitMilliseconds = waitTime * 1000f;
        var waitMillisecondsInt = (int)waitMilliseconds;

        if (waitMillisecondsInt > 0)
        {
            Thread.Sleep(waitMillisecondsInt);
        }

        // lost time is added to the remaining time
        _remainingTime = (waitMilliseconds - waitMillisecondsInt) / 1000f;
    }
}