using System;
using System.Diagnostics.CodeAnalysis;
using BepInEx.Logging;

namespace UniTAS.Plugin.Logger;

/// <summary>
/// Logger specifically for TAS movies.
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class MovieLogger : IMovieLogger
{
    private readonly ManualLogSource _logSource = new("Movie playback");

    public MovieLogger()
    {
        BepInEx.Logging.Logger.Sources.Add(_logSource);
    }

    public void LogError(object data)
    {
        _logSource.LogError(data);
    }

    public void LogWarning(object data)
    {
        _logSource.LogWarning(data);
    }

    public void LogInfo(object data)
    {
        _logSource.LogInfo(data);
    }

    public event EventHandler<LogEventArgs> OnLog
    {
        add => _logSource.LogEvent += value;
        remove => _logSource.LogEvent -= value;
    }
}