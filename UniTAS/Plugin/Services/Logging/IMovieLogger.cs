﻿using System;
using BepInEx.Logging;

namespace UniTAS.Plugin.Services.Logging;

public interface IMovieLogger
{
    void LogError(object data);
    void LogInfo(object data);
    void LogWarning(object data);
    event EventHandler<LogEventArgs> OnLog; 
}