using System;
using BepInEx.Logging;

namespace UniTASPlugin.Logger;

/// <summary>
/// Logger specifically for TAS movies.
/// </summary>
public class MovieLogger : IMovieLogger
{
    private readonly ILogger _logger;

    private readonly ManualLogSource _logSource = new("Movie playback");

    public MovieLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void LogError(object data)
    {
        _logger.LogError(data);
        _logSource.LogError(data);
    }

    public void LogWarning(object data)
    {
        _logger.LogWarning(data);
        _logSource.LogWarning(data);
    }

    public void LogInfo(object data)
    {
        _logger.LogInfo(data);
        _logSource.LogInfo(data);
    }

    public void LogDebug(object data)
    {
        _logger.LogDebug(data);
        _logSource.LogDebug(data);
    }

    public event EventHandler<LogEventArgs> OnLog
    {
        add => _logSource.LogEvent += value;
        remove => _logSource.LogEvent -= value;
    }
}