using HarmonyLib;
using System;
using UniTASPlugin.ReversePatches.__UnityEngine;

namespace UniTASPlugin.VersionSafeWrapper;

internal static class CursorWrap
{
    static readonly bool ScreenLockCursorExists = Traverse.Create<UnityEngine.Screen>().Property("lockCursor").PropertyExists();

    static readonly bool ScreenShowCursorExists = Traverse.Create<UnityEngine.Screen>().Property("showCursor").PropertyExists();

    static readonly bool CursorTypeExists = Traverse.CreateWithType("UnityEngine.Cursor").TypeExists();

    static readonly bool LockModeTypeExists = Traverse.CreateWithType("UnityEngine.CursorLockMode").TypeExists();

    static readonly bool CursorLockStateExists = Traverse.CreateWithType("UnityEngine.Cursor").Property("lockState").PropertyExists();

    static readonly Type CursorLockModeType = AccessTools.TypeByName("UnityEngine.CursorLockMode");

    public static bool visible
    {
        get
        {
            if (CursorTypeExists)
            {
                return Cursor.visible;
            }
            if (!ScreenShowCursorExists)
            {
                Plugin.Log.LogError("Failed to retrieve Screen.showCursor property");
                return false;
            }
            return Screen.showCursor;
        }
        set
        {
            if (CursorTypeExists)
            {
                Cursor.visible = value;
                return;
            }
            if (!ScreenShowCursorExists)
            {
                Plugin.Log.LogError("Failed to set Screen.showCursor property");
                return;
            }
            Screen.showCursor = value;
        }
    }

    public static int TempStoreLockVariant;
    public static bool? TempStoreLockCursorState = null;
    public static bool TempUnlocked { get; private set; } = false;

    public static void TempCursorLockToggle(bool unlock)
    {
        TempUnlocked = unlock;
        if (unlock)
        {
            // store data and unlock cursor
            if (CursorTypeExists)
            {
                TempStoreLockVariant = Cursor.lockState;
            }
            else
            {
                if (!ScreenLockCursorExists)
                {
                    Plugin.Log.LogError("Failed to unlock cursor, lockCursor property not found");
                    TempUnlocked = false;
                    return;
                }
                Screen.lockCursor = false;
            }
            UnlockCursor();
        }
        else
        {
            // restore lock state
            if (CursorTypeExists)
            {
                Cursor.lockState = TempStoreLockVariant;
                return;
            }
            if (TempStoreLockCursorState is not null)
                Screen.lockCursor = TempStoreLockCursorState.Value;
        }
    }

    public static void UnlockCursor()
    {
        if (LockModeTypeExists)
        {
            var unlockVariant = "None";
            if (Enum.IsDefined(CursorLockModeType, unlockVariant))
            {
                var variant = Enum.Parse(CursorLockModeType, unlockVariant);
                if (CursorLockStateExists)
                {
                    Cursor.lockState = (int)variant;
                }
                else
                {
                    Plugin.Log.LogError("UnityEngine.CursorLockMode exists but the UnityEngine.Cursor.lockState is missing");
                }
            }
            else
            {
                Plugin.Log.LogError("UnityEngine.CursorLockMode exists but the None variant is missing");
            }
            return;
        }
        if (!ScreenLockCursorExists)
        {
            Plugin.Log.LogError("Failed to unlock cursor, lockCursor property not found");
            return;
        }
        Screen.lockCursor = false;
    }
}
