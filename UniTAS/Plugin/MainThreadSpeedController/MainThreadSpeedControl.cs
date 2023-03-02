using System.Diagnostics;
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
            _speedMultiplier = value;

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
        if (SpeedMultiplier == 0 || !_virtualEnvironment.RunVirtualEnvironment ||
            _virtualEnvironment.FrameTime == 0f) return;

        var timeSinceLastUpdate = CurrentTime - _lastTime;
        _lastTime = CurrentTime;

        // if the actual time passed is less than the time that should have passed, wait
        var waitTime = _virtualEnvironment.FrameTime * SpeedMultiplier - timeSinceLastUpdate + _remainingTime;
        Trace.Write($"Time since last update: {timeSinceLastUpdate} s, wait time: {waitTime} s");
        if (waitTime <= 0)
        {
            Trace.Write(
                $"Returning from MainThreadSpeedControl.Update() because {waitTime} <= 0, new remaining time: {_remainingTime} s");
            return;
        }

        var waitMilliseconds = waitTime * 1000f;
        var waitMillisecondsInt = (int)waitMilliseconds;

        if (waitMillisecondsInt > 0)
        {
            Trace.Write($"Sleep for {waitMillisecondsInt} ms ({waitMilliseconds} ms)");
            Thread.Sleep(waitMillisecondsInt);
        }

        // lost time is added to the remaining time
        _remainingTime = (waitMilliseconds - waitMillisecondsInt) / 1000f;
    }
}