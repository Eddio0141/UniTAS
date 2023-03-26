using System;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events;
using UniTAS.Plugin.Services.Logging;

namespace UniTAS.Plugin.Implementations.Logging;

/// <summary>
/// Logger specifically for TAS movies.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[Singleton]
[ExcludeRegisterIfTesting]
public class MovieLogger : IMovieLogger, IOnMovieRunningStatusChange
{
    private readonly ManualLogSource _logSource = new("Movie playback");
    private bool _movieRunning;

    public void OnMovieRunningStatusChange(bool running)
    {
        _movieRunning = running;
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