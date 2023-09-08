using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Singleton]
[ExcludeRegisterIfTesting]
public class MainThreadSpeedControl : IMainThreadSpeedControl, IOnUpdateUnconditional, IOnMovieRunningStatusChange
{
    private readonly ITimeEnv _timeEnv;
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
    private float _speedMultiplier = 1f;

    private double _remainingTime;

    public MainThreadSpeedControl(IPatchReverseInvoker patchReverseInvoker, ITimeEnv timeEnv)
    {
        _patchReverseInvoker = patchReverseInvoker;
        _timeEnv = timeEnv;
    }

    private float CurrentTime => _patchReverseInvoker.Invoke(() => Time.realtimeSinceStartup);

    public void UpdateUnconditional()
    {
        if (_speedMultiplier == 0) return;

        var timeSinceLastUpdate = CurrentTime - _lastTime;
        _lastTime = CurrentTime;

        // if the actual time passed is less than the time that should have passed, wait
        var waitTime = _timeEnv.FrameTime / _speedMultiplier - timeSinceLastUpdate + _remainingTime;
        if (waitTime <= 0)
        {
            return;
        }

        var waitMilliseconds = waitTime * 1000.0;
        var waitMillisecondsInt = (int)waitMilliseconds;

        if (waitMillisecondsInt > 0)
        {
            Thread.Sleep(waitMillisecondsInt);
        }

        // lost time is added to the remaining time
        _remainingTime = (waitMilliseconds - waitMillisecondsInt) / 1000.0;
    }

    public void OnMovieRunningStatusChange(bool running)
    {
        SpeedMultiplier = running ? 0f : 1f;
    }
}