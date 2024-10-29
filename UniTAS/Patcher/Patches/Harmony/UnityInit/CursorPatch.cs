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
public class CursorPatch
{
    private static readonly IMouseOverlayStatus MouseOverlayStatus =
        ContainerStarter.Kernel.GetInstance<IMouseOverlayStatus>();

    private static readonly IPatchReverseInvoker PatchReverseInvoker =
        ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();

    private static readonly IActualCursorStateUpdate ActualCursorStateUpdate =
        ContainerStarter.Kernel.GetInstance<IActualCursorStateUpdate>();

    private static readonly IToolBar ToolBar = ContainerStarter.Kernel.GetInstance<IToolBar>();

    [HarmonyPatch]
    private class Visible
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

            ActualCursorStateUpdate.CursorVisible = value;
            return !ToolBar.PreventCursorChange;
        }
    }

    [HarmonyPatch]
    private class LockState
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

            ActualCursorStateUpdate.CursorLockState =
                (CursorLockMode)Enum.Parse(typeof(CursorLockMode), value.ToString());
            return !ToolBar.PreventCursorChange;
        }
    }

    [HarmonyPatch]
    private class LockCursor
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

            ActualCursorStateUpdate.CursorLockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            return !ToolBar.PreventCursorChange;
        }
    }
}