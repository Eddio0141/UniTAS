using System;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.GameRestart;

/// <summary>
/// Properly resets unity's Time class to start up state
/// </summary>
public class SoftRestartTimeReset : IOnGameRestart
{
    public void OnGameRestart(DateTime startupTime, bool preSceneLoad)
    {
        if (!preSceneLoad) return;

        Time.timeScale = 1;
    }
}