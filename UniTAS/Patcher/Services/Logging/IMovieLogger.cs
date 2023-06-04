using System;
using BepInEx.Logging;

namespace UniTAS.Patcher.Services.Logging;

public interface IMovieLogger
{
    void LogError(object data, bool whenPlayingMovie = false);
    void LogInfo(object data, bool whenPlayingMovie = false);
    void LogWarning(object data, bool whenPlayingMovie = false);
    event EventHandler<LogEventArgs> OnLog;
}