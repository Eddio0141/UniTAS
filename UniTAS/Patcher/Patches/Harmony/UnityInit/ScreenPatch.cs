using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Implementations.UnitySafeWrappers;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.Trackers.UpdateTrackInfo;
using UniTAS.Patcher.Services.UnitySafeWrappers;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;
using UnityEngine;

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class ScreenPatch
{
    private static readonly IWindowEnv WindowEnv = ContainerStarter.Kernel.GetInstance<IWindowEnv>();

    private static readonly IVirtualEnvController VirtualEnvController =
        ContainerStarter.Kernel.GetInstance<IVirtualEnvController>();

    private static readonly IPatchReverseInvoker PatchReverseInvoker =
        ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();

    private static readonly IScreenTrackerUpdate ScreenTrackerUpdate =
        ContainerStarter.Kernel.GetInstance<IScreenTrackerUpdate>();

    private static readonly IUnityInstanceWrapFactory UnityInstanceWrapFactory =
        ContainerStarter.Kernel.GetInstance<IUnityInstanceWrapFactory>();

    [HarmonyPatch(typeof(Screen), nameof(Screen.resolutions), MethodType.Getter)]
    private class Resolutions
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref Resolution[] __result)
        {
            if (PatchReverseInvoker.Invoking || !VirtualEnvController.RunVirtualEnvironment) return true;
            var allRes = WindowEnv.ExtraSupportedResolutions.AddItem(WindowEnv.CurrentResolution)
                .Select(x => (Resolution)((ResolutionWrapper)x).Instance).ToArray();
            __result = allRes;
            return false;
        }
    }

    [HarmonyPatch(typeof(Screen), nameof(Screen.SetResolution), typeof(int), typeof(int), typeof(bool))]
    private class SetResolutionIntIntBool
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(int width, int height, ref bool fullscreen)
        {
            if (PatchReverseInvoker.Invoking || !VirtualEnvController.RunVirtualEnvironment) return;

            ScreenTrackerUpdate.SetResolution(width, height, fullscreen);
            fullscreen = false;
        }
    }

    [HarmonyPatch(typeof(Screen), nameof(Screen.SetResolution), typeof(int), typeof(int), typeof(bool), typeof(int))]
    private class SetResolutionIntIntBoolInt
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static void Prefix(int width, int height, ref bool fullscreen, int preferredRefreshRate)
        {
            if (PatchReverseInvoker.Invoking || !VirtualEnvController.RunVirtualEnvironment) return;

            ScreenTrackerUpdate.SetResolution(width, height, fullscreen, preferredRefreshRate);
            fullscreen = false;
        }
    }

    [HarmonyPatch]
    private class SetResolutionIntIntFullScreenModeInt
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod()
        {
            var fullScreenModeType = AccessTools.TypeByName("UnityEngine.FullScreenMode");
            if (fullScreenModeType == null) return null;
            return AccessTools.Method(typeof(Screen), nameof(Screen.SetResolution),
                [typeof(int), typeof(int), fullScreenModeType, typeof(int)]);
        }

        private static void Prefix(int width, int height, ref object fullscreenMode, int preferredRefreshRate)
        {
            if (PatchReverseInvoker.Invoking || !VirtualEnvController.RunVirtualEnvironment) return;

            ScreenTrackerUpdate.SetResolution(width, height, fullscreenMode, preferredRefreshRate);
            fullscreenMode = FullScreenModeWrap.FullScreenModeWindowed;
        }
    }

    [HarmonyPatch]
    private class SetResolutionIntIntFullScreenModeRefreshRate
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static MethodBase TargetMethod()
        {
            var fullScreenModeType = AccessTools.TypeByName("UnityEngine.FullScreenMode");
            if (fullScreenModeType == null) return null;
            var refreshRateType = AccessTools.TypeByName("UnityEngine.RefreshRate");
            if (refreshRateType == null) return null;
            return AccessTools.Method(typeof(Screen), nameof(Screen.SetResolution),
                [typeof(int), typeof(int), fullScreenModeType, refreshRateType]);
        }

        private static void Prefix(int width, int height, ref object fullscreenMode, object preferredRefreshRate)
        {
            if (PatchReverseInvoker.Invoking || !VirtualEnvController.RunVirtualEnvironment) return;

            ScreenTrackerUpdate.SetResolution(width, height, fullscreenMode, preferredRefreshRate);
            fullscreenMode = FullScreenModeWrap.FullScreenModeWindowed;
        }
    }

    [HarmonyPatch(typeof(Screen), nameof(Screen.fullScreen), MethodType.Getter)]
    private class GetFullScreen
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref bool __result)
        {
            if (PatchReverseInvoker.Invoking || !VirtualEnvController.RunVirtualEnvironment) return true;
            __result = WindowEnv.FullScreen;
            return false;
        }
    }

    [HarmonyPatch(typeof(Screen), nameof(Screen.fullScreen), MethodType.Setter)]
    private class SetFullScreen
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(bool value)
        {
            if (PatchReverseInvoker.Invoking || !VirtualEnvController.RunVirtualEnvironment) return true;
            WindowEnv.FullScreen = value;
            return false;
        }
    }

    [HarmonyPatch(typeof(Screen), "fullScreenMode", MethodType.Getter)]
    private class GetFullScreenMode
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref object __result)
        {
            if (PatchReverseInvoker.Invoking || !VirtualEnvController.RunVirtualEnvironment) return true;
            __result = WindowEnv.FullScreenMode.Instance;
            return false;
        }
    }

    [HarmonyPatch(typeof(Screen), nameof(Screen.fullScreen), MethodType.Setter)]
    private class SetFullScreenMode
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(object value)
        {
            if (PatchReverseInvoker.Invoking || !VirtualEnvController.RunVirtualEnvironment) return true;
            WindowEnv.FullScreenMode = UnityInstanceWrapFactory.Create<FullScreenModeWrap>(value);
            return false;
        }
    }
}