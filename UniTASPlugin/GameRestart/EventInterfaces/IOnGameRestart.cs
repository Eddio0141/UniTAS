using System;

namespace UniTASPlugin.GameRestart.EventInterfaces;

/// <summary>
/// Called when soft restart is happening.
/// </summary>
public interface IOnGameRestart
{
    void OnGameRestart(DateTime startupTime, bool preSceneLoad);
}