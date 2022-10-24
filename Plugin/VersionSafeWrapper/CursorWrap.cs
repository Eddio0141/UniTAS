using System;
using HarmonyLib;
using UniTASPlugin.ReversePatches.__UnityEngine;
using Screen = UnityEngine.Screen;

namespace UniTASPlugin.VersionSafeWrapper;

internal static class CursorWrap
{
    private static readonly bool ScreenLockCursorExists = Traverse.Create<Screen>().Property("lockCursor").PropertyExists();

    private static readonly bool ScreenShowCursorExists = Traverse.Create<Screen>().Property("showCursor").PropertyExists();

    private static readonly bool CursorTypeExists = Traverse.CreateWithType("UnityEngine.Cursor").TypeExists();

    private static readonly bool LockModeTypeExists = Traverse.CreateWithType("UnityEngine.CursorLockMode").TypeExists();

    private static readonly bool CursorLockStateExists = Traverse.CreateWithType("UnityEngine.Cursor").Property("lockState").PropertyExists();

    private static readonly Type CursorLockModeType = AccessTools.TypeByName("UnityEngine.CursorLockMode");

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
                Plugin.Instance.Log.LogError("Failed to retrieve Screen.showCursor property");
                return false;
            }
            return ReversePatches.__UnityEngine.Screen.showCursor;
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
                Plugin.Instance.Log.LogError("Failed to set Screen.showCursor property");
                return;
            }
            ReversePatches.__UnityEngine.Screen.showCursor = value;
        }
    }

    public static int TempStoreLockVariant;
    public static bool? TempStoreLockCursorState = null;
    public static bool TempUnlocked { get; private set; }

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
                    Plugin.Instance.Log.LogError("Failed to unlock cursor, lockCursor property not found");
                    TempUnlocked = false;
                    return;
                }
                ReversePatches.__UnityEngine.Screen.lockCursor = false;
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
                ReversePatches.__UnityEngine.Screen.lockCursor = TempStoreLockCursorState.Value;
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
                    Plugin.Instance.Log.LogError("UnityEngine.CursorLockMode exists but the UnityEngine.Cursor.lockState is missing");
                }
            }
            else
            {
                Plugin.Instance.Log.LogError("UnityEngine.CursorLockMode exists but the None variant is missing");
            }
            return;
        }
        if (!ScreenLockCursorExists)
        {
            Plugin.Instance.Log.LogError("Failed to unlock cursor, lockCursor property not found");
            return;
        }
        ReversePatches.__UnityEngine.Screen.lockCursor = false;
    }
}
