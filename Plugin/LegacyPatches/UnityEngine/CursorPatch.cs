using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.LegacyGameOverlay;
using UniTASPlugin.ReverseInvoker;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace UniTASPlugin.Patches.UnityEngine;

[HarmonyPatch]
internal static class CursorPatch
{
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
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static void Prefix(ref bool value)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return;
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
            return AccessTools.Method(CursorHelper.CursorType(), "SetCursor",
                new[] { typeof(Texture2D), typeof(Vector2), CursorHelper.CursorLockMode() });
        }

        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static void Prefix(Texture2D texture)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return;
            Overlay.SetCursorTexture(texture);
        }
    }
}