using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony;

[RawPatch]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class FpsUnlockPatch
{
    private static readonly IPatchReverseInvoker
        PatchReverseInvoker = ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();

    private static readonly IGameSpeedUnlocker GameSpeedUnlocker =
        ContainerStarter.Kernel.GetInstance<IGameSpeedUnlocker>();

    [HarmonyPatch(typeof(Application), nameof(Application.targetFrameRate), MethodType.Getter)]
    private class ApplicationTargetFrameRateGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (PatchReverseInvoker.InnerCall() || !GameSpeedUnlocker.Unlock) return true;

            // modify return to be original if we are not reverse invoking or if we are not unlocking
            __result = GameSpeedUnlocker.OriginalTargetFrameRate;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(Application), nameof(Application.targetFrameRate), MethodType.Setter)]
    private class ApplicationTargetFrameRateSetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(int value)
        {
            if (PatchReverseInvoker.InnerCall() && !GameSpeedUnlocker.Unlock) return true;

            // ignore set if we are not reverse invoking or if we are not unlocking
            GameSpeedUnlocker.OriginalTargetFrameRate = value;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(QualitySettings), nameof(QualitySettings.vSyncCount), MethodType.Getter)]
    private class VsyncGetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (PatchReverseInvoker.InnerCall() || !GameSpeedUnlocker.Unlock) return true;

            // modify return to be original if we are not reverse invoking or if we are not unlocking
            __result = GameSpeedUnlocker.OriginalVSyncCount;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }

    [HarmonyPatch(typeof(QualitySettings), nameof(QualitySettings.vSyncCount), MethodType.Setter)]
    private class VsyncSetter
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(int value)
        {
            if (PatchReverseInvoker.InnerCall() && !GameSpeedUnlocker.Unlock) return true;

            // ignore set if we are not reverse invoking or if we are not unlocking
            GameSpeedUnlocker.OriginalVSyncCount = value;
            return false;
        }

        private static void Postfix()
        {
            PatchReverseInvoker.Return();
        }
    }
}