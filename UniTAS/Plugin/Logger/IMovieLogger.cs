using System;
using BepInEx.Logging;

namespace UniTAS.Plugin.Logger;

public interface IMovieLogger
{
    void LogError(object data);
    void LogInfo(object data);
    event EventHandler<LogEventArgs> OnLog; 
}