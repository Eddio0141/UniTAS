using System;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Movie;
using UniTAS.Patcher.Services.NewInputSystem;

namespace UniTAS.Patcher.Implementations.NewInputSystem;

[Singleton]
[ForceInstantiate]
public class OverrideInputOnMovie
{
    private readonly IInputSystemOverride _inputSystemOverride;

    public OverrideInputOnMovie(IGameRestart gameRestart, IInputSystemOverride inputSystemOverride,
        IMovieRunnerEvents movieEvents)
    {
        _inputSystemOverride = inputSystemOverride;
        gameRestart.OnGameRestartResume += OnGameRestartResume;
        movieEvents.OnMovieEnd += OnMovieEnd;
    }

    private void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume)
    {
        if (!preMonoBehaviourResume) return;
        _inputSystemOverride.Override = true;
    }

    private void OnMovieEnd()
    {
        _inputSystemOverride.Override = false;
    }
}