using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using UniTAS.Patcher.Interfaces.Patches.PatchTypes;
using UniTAS.Patcher.Services;
using UniTAS.Patcher.Services.VirtualEnvironment;
using UniTAS.Patcher.Utils;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

// ReSharper disable ClassNeverInstantiated.Global

namespace UniTAS.Patcher.Patches.Harmony.UnityInit;

[RawPatchUnityInit]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
public class SystemTimeOverrideModule
{
    private static readonly IPatchReverseInvoker ReverseInvoker =
        ContainerStarter.Kernel.GetInstance<IPatchReverseInvoker>();

    private static readonly ITimeEnv TimeEnv =
        ContainerStarter.Kernel.GetInstance<ITimeEnv>();

    [HarmonyPatch(typeof(DateTime), nameof(DateTime.Now), MethodType.Getter)]
    private class get_Now
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref DateTime __result)
        {
            if (ReverseInvoker.Invoking)
                return true;
            __result = TimeEnv.CurrentTime;
            return false;
        }
    }

    [HarmonyPatch(typeof(Environment), nameof(Environment.TickCount), MethodType.Getter)]
    private class get_TickCount
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatchHelper.CleanupIgnoreFail(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            __result = (int)(TimeEnv.RealtimeSinceStartup * 1000f);
            return false;
        }
    }
}