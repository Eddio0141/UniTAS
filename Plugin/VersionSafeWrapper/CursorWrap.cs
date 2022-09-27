using HarmonyLib;
using System;

namespace UniTASPlugin.VersionSafeWrapper;

internal static class CursorWrap
{
    public static bool SettingCursorVisible { get; private set; } = false;

    static Type cursorType()
    {
        return AccessTools.TypeByName("UnityEngine.Cursor");
    }

    static Type screenType()
    {
        return AccessTools.TypeByName("UnityEngine.Screen");
    }

    static Type cursorLockModeType()
    {
        return AccessTools.TypeByName("UnityEngine.CursorLockMode");
    }

    public static bool visible
    {
        get
        {
            var cursor = Traverse.Create(cursorType());
            if (cursor.TypeExists())
            {
                return cursor.Property("visible").GetValue<bool>();
            }
            var screen = Traverse.Create(screenType());
            var showCursor = screen.Property("showCursor");
            if (!showCursor.PropertyExists())
            {
                Plugin.Log.LogError("Failed to retrieve Screen.showCursor property");
                return false;
            }
            return showCursor.GetValue<bool>();
        }
        set
        {
            SettingCursorVisible = true;
            var cursor = Traverse.Create(cursorType());
            if (cursor.TypeExists())
            {
                cursor.Property("visible").SetValue(value);
                SettingCursorVisible = false;
                return;
            }
            var screen = Traverse.Create(screenType());
            var showCursor = screen.Property("showCursor");
            if (!showCursor.PropertyExists())
            {
                Plugin.Log.LogError("Failed to set Screen.showCursor property");
                SettingCursorVisible = false;
                return;
            }
            showCursor.SetValue(value);
            SettingCursorVisible = false;
        }
    }

    public static object TempStoreLockVariant = null;
    public static bool? TempStoreLockCursorState = null;
    public static bool TempUnlocked { get; private set; } = false;

    public static void TempCursorLockToggle(bool unlock)
    {
        TempUnlocked = unlock;
        if (unlock)
        {
            // store data and unlock cursor
            var cursor = Traverse.Create(cursorType());
            if (cursor.TypeExists())
            {
                TempStoreLockVariant = cursor.Property("lockState").GetValue();
            }
            else
            {
                var lockCursor = Traverse.Create(screenType()).Property("lockCursor");
                if (!lockCursor.PropertyExists())
                {
                    Plugin.Log.LogError("Failed to unlock cursor, lockCursor property not found");
                    TempUnlocked = false;
                    return;
                }
                lockCursor.SetValue(false);
            }
            UnlockCursor();
        }
        else
        {
            // restore lock state
            var cursor = Traverse.Create(cursorType());
            if (cursor.TypeExists())
            {
                if (TempStoreLockVariant != null)
                    cursor.Property("lockState").SetValue(TempStoreLockVariant);
                return;
            }
            if (TempStoreLockCursorState != null)
                Traverse.Create(screenType()).Property("lockCursor").SetValue((bool)TempStoreLockCursorState);
        }
    }

    public static void UnlockCursor()
    {
        var lockModeType = cursorLockModeType();
        var lockMode = Traverse.Create(lockModeType);
        if (lockMode.TypeExists())
        {
            var unlockVariant = "None";
            if (Enum.IsDefined(lockModeType, unlockVariant))
            {
                var variant = Enum.Parse(lockModeType, unlockVariant);
                var cursor = Traverse.Create(cursorType());
                var lockState = cursor.Property("lockState");
                if (lockState.PropertyExists())
                {
                    lockState.SetValue(variant);
                    return;
                }
            }
        }

        var screen = Traverse.Create(screenType());
        var lockCursor = screen.Property("lockCursor");
        if (!lockCursor.PropertyExists())
        {
            Plugin.Log.LogError("Failed to unlock cursor, lockCursor property not found");
            return;
        }
        lockCursor.SetValue(false);
    }
}
