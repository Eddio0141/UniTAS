using System;
using BepInEx.Logging;

namespace UniTAS.Plugin.Logger;

public interface IMovieLogger
{
    void LogError(object data, bool whenPlayingMovie = false);
    void LogInfo(object data, bool whenPlayingMovie = false);
    void LogWarning(object data, bool whenPlayingMovie = false);
    event EventHandler<LogEventArgs> OnLog;
}