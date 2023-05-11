using System;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Services;
using UniTAS.Plugin.Services.Movie;
using UniTAS.Plugin.Services.NewInputSystem;

namespace UniTAS.Plugin.Implementations.NewInputSystem;

[Singleton]
public class OverrideInputOnMovie
{
    private readonly IInputSystemOverride _inputSystemOverride;

    public OverrideInputOnMovie(IGameRestart gameRestart, IInputSystemOverride inputSystemOverride,
        IMovieRunner movieRunner)
    {
        _inputSystemOverride = inputSystemOverride;
        gameRestart.OnGameRestart += OnGameRestart;
        movieRunner.OnMovieEnd += OnMovieEnd;
    }

    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (!preSceneLoad) return;

        _inputSystemOverride.Override = true;
    }

    private void OnMovieEnd()
    {
        _inputSystemOverride.Override = false;
    }
}