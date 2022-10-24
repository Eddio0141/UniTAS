using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.GameOverlay;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin.Patches.UnityEngine;

internal class CursorHelper
{
    public static Type CursorType()
    {
        return AccessTools.TypeByName("UnityEngine.Cursor");
    }

    public static Type CursorLockMode()
    {
        return AccessTools.TypeByName("UnityEngine.CursorLockMode");
    }
}

[HarmonyPatch]
internal class set_visible
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.PropertySetter(CursorHelper.CursorType(), "visible");
    }

    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static void Prefix(ref bool value)
    {
        Overlay.UnityCursorVisible = value;
        if (Overlay.ShowCursor)
            value = false;
    }
}

[HarmonyPatch]
internal class SetCursor
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.Method(CursorHelper.CursorType(), "SetCursor", new[] { typeof(Texture2D), typeof(Vector2), CursorHelper.CursorLockMode() });
    }

    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static void Prefix(Texture2D texture)
    {
        Overlay.SetCursorTexture(texture);
    }
}

[HarmonyPatch]
internal class set_lockState
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.PropertySetter(CursorHelper.CursorType(), "lockState");
    }

    private static Exception Cleanup(MethodBase original, Exception ex)
    {
        return PatcherHelper.Cleanup_IgnoreException(original, ex);
    }

    private static void Prefix(object value)
    {
        if (CursorWrap.TempUnlocked)
            CursorWrap.TempStoreLockVariant = (int)value;
    }
}
