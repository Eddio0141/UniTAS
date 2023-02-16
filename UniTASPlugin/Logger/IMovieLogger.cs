using System;
using BepInEx.Logging;

namespace UniTASPlugin.Logger;

public interface IMovieLogger
{
    void LogError(object data);
    void LogInfo(object data);
    event EventHandler<LogEventArgs> OnLog; 
}