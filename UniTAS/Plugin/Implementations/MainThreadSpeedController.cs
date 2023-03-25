using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UniTAS.Plugin.Interfaces.Events;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.VirtualEnvironment;
using UnityEngine;

namespace UniTAS.Plugin.Implementations;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class MainThreadSpeedControl : IMainThreadSpeedControl, IOnUpdate, IOnMovieRunningStatusChange
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
    private float _speedMultiplier;

    private float _remainingTime;

    public MainThreadSpeedControl(IPatchReverseInvoker patchReverseInvoker, ITimeEnv timeEnv)
    {
        _patchReverseInvoker = patchReverseInvoker;
        _timeEnv = timeEnv;
    }

    private float CurrentTime => _patchReverseInvoker.Invoke(() => Time.realtimeSinceStartup);

    public void Update()
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

        var waitMilliseconds = waitTime * 1000f;
        var waitMillisecondsInt = (int)waitMilliseconds;

        if (waitMillisecondsInt > 0)
        {
            Thread.Sleep(waitMillisecondsInt);
        }

        // lost time is added to the remaining time
        _remainingTime = (waitMilliseconds - waitMillisecondsInt) / 1000f;
    }

    public void OnMovieRunningStatusChange(bool running)
    {
        SpeedMultiplier = running ? 0f : 1f;
    }
}