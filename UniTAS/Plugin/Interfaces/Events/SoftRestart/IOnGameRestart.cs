using System;

namespace UniTAS.Plugin.Interfaces.Events.SoftRestart;

/// <summary>
/// Called when soft restart is happening.
/// </summary>
public interface IOnGameRestart
{
    void OnGameRestart(DateTime startupTime, bool preSceneLoad);
}