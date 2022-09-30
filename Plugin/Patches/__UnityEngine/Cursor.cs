using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.GameOverlay;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

class CursorHelper
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
class set_visible
{
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertySetter(CursorHelper.CursorType(), "visible");
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(ref bool value)
    {
        Overlay.UnityCursorVisible = value;
        if (Overlay.ShowCursor)
            value = false;
    }
}

[HarmonyPatch]
class SetCursor
{
    static MethodBase TargetMethod()
    {
        return AccessTools.Method(CursorHelper.CursorType(), "SetCursor", new Type[] { typeof(Texture2D), typeof(Vector2), CursorHelper.CursorLockMode() });
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(Texture2D texture)
    {
        Overlay.SetCursorTexture(texture);
    }
}

[HarmonyPatch]
class set_lockState
{
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertySetter(CursorHelper.CursorType(), "lockState");
    }

    static Exception Cleanup(MethodBase original, Exception ex)
    {
        return AuxilaryHelper.Cleanup_IgnoreException(original, ex);
    }

    static void Prefix(object value)
    {
        if (CursorWrap.TempUnlocked)
            CursorWrap.TempStoreLockVariant = (int)value;
    }
}
