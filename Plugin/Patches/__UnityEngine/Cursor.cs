using HarmonyLib;
using System;
using System.Reflection;
using UniTASPlugin.VersionSafeWrapper;
using UnityEngine;

namespace UniTASPlugin.Patches.__UnityEngine;

class CursorHelper
{
    public static Type CursorType()
    {
        return AccessTools.TypeByName("UnityEngine.Cursor");
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
        if (!CursorWrap.SettingCursorVisible)
        {
            Overlay.UnityCursorVisible = value;
            if (Overlay.ShowCursor)
                value = false;
        }
    }
}

[HarmonyPatch]
class SetCursor
{
    static MethodBase TargetMethod()
    {
        var cursorModeType = AccessTools.TypeByName("UnityEngine.CursorMode");
        return AccessTools.Method(CursorHelper.CursorType(), "SetCursor", new Type[] { typeof(Texture2D), typeof(Vector2), cursorModeType });
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
