using System;

namespace UniTAS.Plugin.GameRestart.EventInterfaces;

/// <summary>
/// Called when the got restarted, and is about to resume execution or has already resumed execution.
/// </summary>
public interface IOnGameRestartResume
{
    void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume);
}