using System;

namespace UniTAS.Patcher.Interfaces.Events.SoftRestart;

/// <summary>
/// Called when soft restart is happening.
/// </summary>
public interface IOnGameRestart
{
    void OnGameRestart(DateTime startupTime, bool preSceneLoad);
}