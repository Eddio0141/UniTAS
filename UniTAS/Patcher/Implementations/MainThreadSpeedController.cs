using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Interfaces.Events.UnityEvents;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Services.GameExecutionControllers;
using UniTAS.Patcher.Services.VirtualEnvironment;

namespace UniTAS.Patcher.Implementations;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Singleton]
[ExcludeRegisterIfTesting]
public class MainThreadSpeedControl(ITimeEnv timeEnv)
    : IMainThreadSpeedControl, IOnUpdateUnconditional, IOnMovieRunningStatusChange, IOnSceneLoad
{
    public float SpeedMultiplier
    {
        get => _speedMultiplier;
        set
        {
            _speedMultiplier = value < 0f ? 0f : value;
            _actualTime = Stopwatch.StartNew();
            _gameTime = 0.0;
        }
    }

    private Stopwatch _actualTime = Stopwatch.StartNew();
    private double _gameTime;
    private float _speedMultiplier = 1f;

    public void OnSceneLoad()
    {
        _actualTime = Stopwatch.StartNew();
        _gameTime = 0.0;
    }

    public void UpdateUnconditional()
    {
        if (_speedMultiplier == 0) return;

        _gameTime += timeEnv.FrameTime / _speedMultiplier;
        var actualTime = _actualTime.Elapsed.TotalSeconds;

        // if the actual time passed is less than the time that should have passed, wait
        var waitTime = _gameTime - actualTime;

        if (waitTime > 0)
        {
            SleepPrecise(waitTime);
        }
    }

    /// <summary>
    /// Precisely sleeps for the specified duration using a hybrid spin-wait approach
    /// </summary>
    /// <param name="seconds">The desired sleep duration</param>
    /// <returns>The actual time slept in milliseconds</returns>
    private static void SleepPrecise(double seconds)
    {
        if (seconds <= 0) return;

        var sw = Stopwatch.StartNew();

        // For very short durations, just spin
        if (seconds <= 0.001)
        {
            while (sw.Elapsed.TotalSeconds < seconds)
            {
                Thread.SpinWait(1);
            }

            sw.Stop();
            return;
        }

        // For longer durations, use a hybrid approach:
        // 1. Sleep for most of the time
        // 2. Spin for the remainder

        var remainingTime = seconds;
        // Sleep until 1ms before target
        while (remainingTime > 0.001)
        {
            Thread.Sleep(1);
            remainingTime = seconds - sw.Elapsed.TotalSeconds;
        }

        // Spin for the final sub-millisecond precision
        while (sw.Elapsed.TotalSeconds < seconds)
        {
            Thread.SpinWait(1);
        }

        sw.Stop();
    }

    public void OnMovieRunningStatusChange(bool running)
    {
        SpeedMultiplier = running ? 0f : 1f;
    }
}