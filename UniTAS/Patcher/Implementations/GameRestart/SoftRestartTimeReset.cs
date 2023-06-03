using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GameRestart;

/// <summary>
/// Properly resets unity's Time class to start up state
/// </summary>
[Singleton]
public class SoftRestartTimeReset : IOnPreGameRestart
{
    public void OnPreGameRestart()
    {
        Time.timeScale = 1;
    }
}