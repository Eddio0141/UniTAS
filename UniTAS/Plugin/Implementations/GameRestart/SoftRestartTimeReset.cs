using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.SoftRestart;
using UnityEngine;

namespace UniTAS.Plugin.Implementations.GameRestart;

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