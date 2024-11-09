using System;
using BepInEx.Logging;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.Movie;
using UniTAS.Patcher.Services.Logging;

namespace UniTAS.Patcher.Implementations.Logging;

/// <summary>
/// Logger specifically for TAS movies.
/// </summary>
[Singleton]
[ExcludeRegisterIfTesting]
public class MovieLogger : IMovieLogger, IOnMovieStart, IOnMovieEnd
{
    private readonly ManualLogSource _logSource = new("Movie playback");
    private bool _movieRunning;

    public void OnMovieRunningStatusChange(bool running)
    {
        _movieRunning = running;
    }

    public void OnMovieStart()
    {
        _movieRunning = true;
    }

    public void OnMovieEnd()
    {
        _movieRunning = false;
    }

    public MovieLogger()
    {
        BepInEx.Logging.Logger.Sources.Add(_logSource);
    }

    public void LogError(object data, bool whenPlayingMovie = false)
    {
        if (whenPlayingMovie && !_movieRunning) return;
        _logSource.LogError(data);
    }

    public void LogWarning(object data, bool whenPlayingMovie = false)
    {
        if (whenPlayingMovie && !_movieRunning) return;
        _logSource.LogWarning(data);
    }

    public void LogInfo(object data, bool whenPlayingMovie = false)
    {
        if (whenPlayingMovie && !_movieRunning) return;
        _logSource.LogInfo(data);
    }

    public event EventHandler<LogEventArgs> OnLog
    {
        add => _logSource.LogEvent += value;
        remove => _logSource.LogEvent -= value;
    }
}