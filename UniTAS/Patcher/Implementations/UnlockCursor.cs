using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.UnitySafeWrappers;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Logging;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ExcludeRegisterIfTesting]
public class UnlockCursor(ICursorWrapper cursorWrapper, ILogger logger, IGameRestart gameRestart)
    : IOnUpdateUnconditional
{
    // private readonly IPatchReverseInvoker _patchReverseInvoker = patchReverseInvoker;

    public void UpdateUnconditional()
    {
        // prevent null ref exception
        if (gameRestart.Restarting) return;

        var input = BepInEx.UnityInput.Current;
        // TODO: fix this
        // if (!_patchReverseInvoker.Invoke(() => input.GetKeyDown(KeyCode.F1))) return;
        if (!input.GetKeyDown(KeyCode.F1)) return;

        cursorWrapper.Visible = true;
        cursorWrapper.LockState = CursorLockMode.None;

        logger.LogDebug("unlocked cursor");
    }
}
