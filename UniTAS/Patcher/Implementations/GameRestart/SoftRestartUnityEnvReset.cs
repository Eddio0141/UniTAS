using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.SoftRestart;
using UniTAS.Patcher.Models.UnitySafeWrappers;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations.GameRestart;

/// <summary>
/// Reset unity environment which can't be reset by soft restart
/// </summary>
[Singleton]
public class SoftRestartUnityEnvReset : IOnPreGameRestart
{
    private readonly ICursorWrapper _cursorWrapper;

    public SoftRestartUnityEnvReset(ICursorWrapper cursorWrapper)
    {
        _cursorWrapper = cursorWrapper;
    }

    public void OnPreGameRestart()
    {
        Time.timeScale = 1;

        _cursorWrapper.Visible = true;
        _cursorWrapper.LockState = CursorLockMode.None;
    }
}