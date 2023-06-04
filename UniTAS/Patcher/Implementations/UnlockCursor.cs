using System;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.DependencyInjection;
using UniTAS.Patcher.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UniTAS.Patcher.Services;
using UnityEngine;

namespace UniTAS.Patcher.Implementations;

[Singleton]
public class UnlockCursor : IOnUpdateUnconditional
{
    private readonly IPatchReverseInvoker _patchReverseInvoker;

    public UnlockCursor(IPatchReverseInvoker patchReverseInvoker)
    {
        _patchReverseInvoker = patchReverseInvoker;
    }

    public void UpdateUnconditional()
    {
        if (!_patchReverseInvoker.Invoke(() => BepInEx.UnityInput.Current.GetKeyDown(KeyCode.F1))) return;

        var cursor = AccessTools.TypeByName("UnityEngine.Cursor");
        if (cursor == null)
        {
            var showCursor = AccessTools.Property(typeof(Screen), "showCursor");
            showCursor.SetValue(null, true, null);
            var lockCursor = AccessTools.Property(typeof(Screen), "lockCursor");
            lockCursor.SetValue(null, false, null);
        }
        else
        {
            var lockState = AccessTools.Property(cursor, "lockState");
            var lockMode = AccessTools.TypeByName("UnityEngine.CursorLockMode");
            var none = Enum.Parse(lockMode, "None");
            lockState.SetValue(null, none, null);

            var visible = AccessTools.Property(cursor, "visible");
            visible.SetValue(null, true, null);
        }
    }
}