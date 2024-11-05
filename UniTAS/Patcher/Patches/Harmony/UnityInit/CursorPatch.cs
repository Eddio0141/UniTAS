using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Models.UnitySafeWrappers;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.GUI;
using UniTAS.Patcher.Services.Overlay;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CursorPatch
{
    private static readonly IMouseOverlayStatus MouseOverlayStatus =
        ContainerStarter.Kernel.GetInstance<IMouseOverlayStatus>();

    private static readonly IPatchReverseInvoker PatchReverseInvoker =
        ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();

    private static readonly IActualCursorState ActualCursorState =
        ContainerStarter.Kernel.GetInstance<IActualCursorState>();

    private static readonly IToolBar ToolBar = ContainerStarter.Kernel.GetInstance<IToolBar>();

    [HarmonyPatch]
    private class SetVisible
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod()
        {
            var cursor = AccessTools.TypeByName("UnityEngine.Cursor");
            return cursor != null
                ? AccessTools.Property(cursor, "visible").GetSetMethod()
                : AccessTools.Property(typeof(Screen), "showCursor").GetSetMethod();
        }

        private static bool Prefix(bool value)
        {
            MouseOverlayStatus.Visible = value;

            if (PatchReverseInvoker.Invoking)
            {
                return true;
            }

            ActualCursorState.CursorVisible = value;
            return !ToolBar.PreventCursorChange;
        }
    }

    [HarmonyPatch]
    private class GetVisible
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod()
        {
            var cursor = AccessTools.TypeByName("UnityEngine.Cursor");
            return cursor != null
                ? AccessTools.Property(cursor, "visible").GetGetMethod()
                : AccessTools.Property(typeof(Screen), "showCursor").GetGetMethod();
        }

        private static bool Prefix(ref bool __result)
        {
            if (!ToolBar.PreventCursorChange) return true;

            __result = ActualCursorState.CursorVisible;
            return false;
        }
    }

    [HarmonyPatch]
    private class SetLockState
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod()
        {
            var cursor = AccessTools.TypeByName("UnityEngine.Cursor");
            return AccessTools.Property(cursor, "lockState").GetSetMethod();
        }

        private static bool Prefix(object value)
        {
            if (PatchReverseInvoker.Invoking)
            {
                return true;
            }

            ActualCursorState.CursorLockState =
                (CursorLockMode)Enum.Parse(typeof(CursorLockMode), value.ToString());
            return !ToolBar.PreventCursorChange;
        }
    }

    [HarmonyPatch]
    private class GetLockState
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod()
        {
            var cursor = AccessTools.TypeByName("UnityEngine.Cursor");
            return AccessTools.Property(cursor, "lockState").GetGetMethod();
        }

        private static readonly Type CursorLockState = AccessTools.TypeByName("UnityEngine.CursorLockMode");

        private static bool Prefix(ref object __result)
        {
            if (!ToolBar.PreventCursorChange) return true;

            __result = Enum.Parse(CursorLockState, ActualCursorState.CursorLockState.ToString());
            return false;
        }
    }

    [HarmonyPatch]
    private class SetLockCursor
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.Property(typeof(Screen), "lockCursor").GetSetMethod();
        }

        private static bool Prefix(bool value)
        {
            if (PatchReverseInvoker.Invoking)
            {
                return true;
            }

            ActualCursorState.CursorLockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            return !ToolBar.PreventCursorChange;
        }
    }
    
    [HarmonyPatch]
    private class GetLockCursor
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.Property(typeof(Screen), "lockCursor").GetGetMethod();
        }

        private static bool Prefix(ref bool __result)
        {
            if (!ToolBar.PreventCursorChange) return true;

            __result = ActualCursorState.CursorLockState == CursorLockMode.Locked;
            return false;
        }
    }
}