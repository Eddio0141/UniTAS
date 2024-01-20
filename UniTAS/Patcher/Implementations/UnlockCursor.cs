using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.UnityEvents.RunEvenPaused;
using UniTAS.Patcher.Models.UnitySafeWrappers;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.UnitySafeWrappers.Wrappers;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton]
[ExcludeRegisterIfTesting]
public class UnlockCursor : IOnUpdateUnconditional
{
    private readonly IPatchReverseInvoker _patchReverseInvoker;
    private readonly ICursorWrapper _cursorWrapper;

    public UnlockCursor(IPatchReverseInvoker patchReverseInvoker, ICursorWrapper cursorWrapper)
    {
        _patchReverseInvoker = patchReverseInvoker;
        _cursorWrapper = cursorWrapper;
    }

    public void UpdateUnconditional()
    {
        if (!_patchReverseInvoker.Invoke(() => BepInEx.UnityInput.Current.GetKeyDown(KeyCode.F1))) return;

        _cursorWrapper.Visible = true;
        _cursorWrapper.LockState = CursorLockMode.None;
    }
}