using System;
using HarmonyLib;
using UniTAS.Plugin.Interfaces.DependencyInjection;
using UniTAS.Plugin.Interfaces.Events.MonoBehaviourEvents.RunEvenPaused;
using UnityEngine;

namespace UniTAS.Plugin.Implementations;

[Singleton]
public class UnlockCursor : IOnUpdateUnconditional
{
    public void UpdateUnconditional()
    {
        if (!BepInEx.UnityInput.Current.GetKeyDown(KeyCode.F1)) return;

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