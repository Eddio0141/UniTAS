using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services.Overlay;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony;

[RawPatch]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class CursorStatusTrackerPatch
{
    private static readonly IMouseOverlayStatus MouseOverlayStatus =
        ContainerStarter.Kernel.GetInstance<IMouseOverlayStatus>();

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

        private static void Prefix(bool value)
        {
            MouseOverlayStatus.Visible = value;
        }
    }
}