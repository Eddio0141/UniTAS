using System;
using HarmonyLib;
using Ninject;
using Screen = UnityEngine.Screen;

namespace UniTASPlugin.VersionSafeWrapper;

internal static class CursorWrap
{
    private static readonly bool ScreenLockCursorExists = ScreenLockCursorTraverse.PropertyExists();
    private static readonly bool ScreenShowCursorExists = ScreenShowCursorTraverse.PropertyExists();
    private static readonly bool CursorTypeExists = CursorTraverse.TypeExists();
    private static readonly bool LockModeTypeExists = Traverse.CreateWithType("UnityEngine.CursorLockMode").TypeExists();
    private static readonly bool CursorLockStateExists = CursorLockStateTraverse.PropertyExists();
    private static readonly Type CursorLockModeType = AccessTools.TypeByName("UnityEngine.CursorLockMode");
    private static Traverse CursorTraverse => Traverse.CreateWithType("UnityEngine.Cursor");
    private static Traverse CursorVisibleTraverse => CursorTraverse.Property("visible");
    private static Traverse CursorLockStateTraverse => CursorTraverse.Property("lockState");
    private static Traverse ScreenTraverse => Traverse.Create<Screen>();
    private static Traverse ScreenShowCursorTraverse => ScreenTraverse.Property("showCursor");
    private static Traverse ScreenLockCursorTraverse => ScreenTraverse.Property("lockCursor");

    public static bool Visible
    {
        get
        {
            if (CursorTypeExists)
            {
                return CursorVisibleTraverse.GetValue<bool>();
            }

            if (ScreenShowCursorExists) return ScreenShowCursorTraverse.GetValue<bool>();
            Plugin.Instance.Log.LogError("Failed to retrieve Screen.showCursor property");
            return false;
        }
        set
        {
            if (CursorTypeExists)
            {
                CursorVisibleTraverse.SetValue(value);
                return;
            }
            if (!ScreenShowCursorExists)
            {
                Plugin.Instance.Log.LogError("Failed to set Screen.showCursor property");
                return;
            }
            ScreenShowCursorTraverse.SetValue(value);
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
                    CursorLockStateTraverse.SetValue((int)variant);
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
        ScreenLockCursorTraverse.SetValue(false);
    }
}
