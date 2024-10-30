using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.NoRefresh;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class CameraPatch
{
    private static readonly IUpdateCameraInfo UpdateCameraInfo =
        ContainerStarter.Kernel.GetInstance<IUpdateCameraInfo>();

    private static readonly IOverridingCameraInfo OverridingCameraInfo =
        ContainerStarter.Kernel.GetInstance<IOverridingCameraInfo>();

    private static readonly IPatchReverseInvoker PatchReverseInvoker =
        ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();

    [HarmonyPatch(typeof(Camera), nameof(Camera.rect), MethodType.Setter)]
    private static class SetRect
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(Camera __instance, Rect value)
        {
            if (PatchReverseInvoker.InnerCall()) return true;
            return !UpdateCameraInfo.SetRect(__instance, value);
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(Camera), nameof(Camera.rect), MethodType.Getter)]
    private static class GetRect
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(Camera __instance, ref Rect __result)
        {
            if (PatchReverseInvoker.InnerCall()) return true;
            if (!OverridingCameraInfo.GetRect(__instance, out var value))
            {
                return true;
            }

            __result = value;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }
}