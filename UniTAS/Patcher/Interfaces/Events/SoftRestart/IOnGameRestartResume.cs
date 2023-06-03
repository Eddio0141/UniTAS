using System;

namespace UniTAS.Patcher.Interfaces.Events.SoftRestart;

/// <summary>
/// Called when the got restarted, and is about to resume execution or has already resumed execution.
/// </summary>
public interface IOnGameRestartResume
{
    void OnGameRestartResume(DateTime startupTime, bool preMonoBehaviourResume);
}