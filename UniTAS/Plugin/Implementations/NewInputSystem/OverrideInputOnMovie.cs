using System;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.Movie;
using UniTAS.Plugin.Services.NewInputSystem;

namespace UniTAS.Plugin.Implementations.NewInputSystem;

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