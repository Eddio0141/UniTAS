﻿using System;
using System.Reflection;
using HarmonyLib;
using UniTASPlugin.GameEnvironment;
using UniTASPlugin.ReverseInvoker;
using EnvOrig = System.Environment;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedMember.Local

namespace UniTASPlugin.Patches.System;

[HarmonyPatch]
internal static class EnvironmentPatch
{
    [HarmonyPatch(typeof(EnvOrig), nameof(EnvOrig.TickCount), MethodType.Getter)]
    private class get_TickCount
    {
        private static Exception Cleanup(MethodBase original, Exception ex)
        {
            return PatcherHelper.CleanupIgnoreException(original, ex);
        }

        private static bool Prefix(ref int __result)
        {
            if (Plugin.Kernel.GetInstance<PatchReverseInvoker>().Invoking)
                return true;
            var gameTime = Plugin.Kernel.GetInstance<IVirtualEnvironmentFactory>().GetVirtualEnv().GameTime;
            __result = (int)(gameTime.RealtimeSinceStartup * 1000f);
            return false;
        }
    }
}